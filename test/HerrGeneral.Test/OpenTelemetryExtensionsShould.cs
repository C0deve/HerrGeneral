namespace HerrGeneral.OpenTelemetry.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using global::OpenTelemetry;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;
using Core.Diagnostics;
using OpenTelemetry;
using WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

public class OpenTelemetryExtensionsShould
{
    public record FullPipelineCommand : CommandBase;

    public record FullPipelineEvent(Guid CommandId);

    public class FullPipelineCommandHandler : ICommandHandler<FullPipelineCommand, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(FullPipelineCommand command)
        {
            return ([new FullPipelineEvent(command.Id)], Unit.Default);
        }
    }

    public class FullPipelineWriteSideEventHandler : IEventHandler<FullPipelineEvent>
    {
        public IReadOnlyList<object> Handle(FullPipelineEvent @event) => [];
    }

    public class FullPipelineSyncProjection : IHandleSyncProjection<FullPipelineEvent>
    {
        public void Handle(FullPipelineEvent @event) { }
    }

    public class FullPipelinePostProjection : IHandlePostProjection<FullPipelineEvent>
    {
        public void Handle(FullPipelineEvent @event) { }
    }

    public class FullPipelineSideEffect : IHandleSideEffect<FullPipelineEvent>
    {
        public void Handle(FullPipelineEvent @event) { }
    }

    public record FailingCommand : CommandBase
    {
        public class Handler : ICommandHandler<FailingCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(FailingCommand command)
                => throw new InvalidOperationException("Failing command for OpenTelemetry test.");
        }
    }

    [Fact]
    public void RegisterHerrGeneralActivitySourceInTracerProvider()
    {
        var exportedActivities = new List<Activity>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddHerrGeneralInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        using (var activity = HerrGeneralDiagnostics.StartActivity(HerrGeneralDiagnostics.Activities.ExecuteCommand))
        {
            activity?.SetTag(HerrGeneralDiagnostics.Tags.CommandName, "TestCommand");
        }

        tracerProvider.ForceFlush();

        exportedActivities.ShouldContain(a =>
            a.DisplayName == HerrGeneralDiagnostics.Activities.ExecuteCommand &&
            (string?)a.GetTagItem(HerrGeneralDiagnostics.Tags.CommandName) == "TestCommand");
    }

    [Fact]
    public void RegisterHerrGeneralMeterInMeterProvider()
    {
        var exportedMetrics = new List<Metric>();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddHerrGeneralInstrumentation()
            .AddInMemoryExporter(exportedMetrics)
            .Build();

        HerrGeneralDiagnostics.CommandsTotal.Add(1, new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, "TestCommand"));

        meterProvider.ForceFlush();

        exportedMetrics.Count.ShouldBeGreaterThan(0);
        exportedMetrics.ShouldContain(m => m.Name == "herrgeneral.commands.total");
    }

    [Fact]
    public async Task CaptureCompleteCommandSpansHierarchyWithMediator()
    {
        var exportedActivities = new List<Activity>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddHerrGeneralInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var services = new ServiceCollection()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanSyncProjectionsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanPostProjectionsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanSideEffectsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var result = await mediator.Send(new FullPipelineCommand());
        result.IsSuccess.ShouldBeTrue();

        tracerProvider.ForceFlush();

        // 1. Root command activity
        var rootActivity = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.ExecuteCommand &&
            (string?)a.GetTagItem(HerrGeneralDiagnostics.Tags.CommandName) == nameof(FullPipelineCommand));

        rootActivity.ShouldNotBeNull();
        rootActivity.Status.ShouldBe(ActivityStatusCode.Ok);
        rootActivity.GetTagItem(HerrGeneralDiagnostics.Tags.Status)?.ToString().ShouldBe("Success");

        // 2. Write side dispatch & handler activities
        var writeSideDispatch = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.WriteSideDispatch &&
            a.ParentSpanId == rootActivity.SpanId);
        writeSideDispatch.ShouldNotBeNull();

        var writeSideHandler = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.WriteSideHandleEvent &&
            a.ParentSpanId == writeSideDispatch.SpanId);
        writeSideHandler.ShouldNotBeNull();

        // 3. Sync projections dispatch & handler activities
        var syncProjectionsDispatch = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.SyncProjectionsDispatch &&
            a.ParentSpanId == rootActivity.SpanId);
        syncProjectionsDispatch.ShouldNotBeNull();

        var syncProjectionsHandler = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.SyncProjectionsHandleEvent &&
            a.ParentSpanId == syncProjectionsDispatch.SpanId);
        syncProjectionsHandler.ShouldNotBeNull();

        // 4. Post transaction dispatch & handler activities
        var postTxDispatch = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.PostTransactionDispatch &&
            a.ParentSpanId == rootActivity.SpanId);
        postTxDispatch.ShouldNotBeNull();

        var postTxHandlers = exportedActivities.Where(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.PostTransactionHandleEvent &&
            a.ParentSpanId == postTxDispatch.SpanId).ToList();
        postTxHandlers.Count.ShouldBe(2); // Post projection + Side effect
    }

    [Fact]
    public async Task CaptureExceptionSpansWithOpenTelemetrySdk()
    {
        var exportedActivities = new List<Activity>();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddHerrGeneralInstrumentation()
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var services = new ServiceCollection()
            .AddHerrGeneral(configuration =>
                configuration.ScanWriteSideOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var result = await mediator.Send(new FailingCommand());
        result.IsPanicError.ShouldBeTrue();

        tracerProvider.ForceFlush();

        var rootActivity = exportedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.ExecuteCommand &&
            (string?)a.GetTagItem(HerrGeneralDiagnostics.Tags.CommandName) == nameof(FailingCommand));

        rootActivity.ShouldNotBeNull();
        rootActivity.Status.ShouldBe(ActivityStatusCode.Error);
        rootActivity.GetTagItem(HerrGeneralDiagnostics.Tags.Status)?.ToString().ShouldBe("Failed");
        rootActivity.Events.ShouldContain(e => e.Name == "exception");
    }

    [Fact]
    public async Task CaptureMetricsWithOpenTelemetrySdkViaMediator()
    {
        var exportedMetrics = new List<Metric>();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddHerrGeneralInstrumentation()
            .AddInMemoryExporter(exportedMetrics)
            .Build();

        var services = new ServiceCollection()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanSyncProjectionsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanPostProjectionsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!)
                    .ScanSideEffectsOn(typeof(OpenTelemetryExtensionsShould).Assembly, typeof(OpenTelemetryExtensionsShould).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        await mediator.Send(new FullPipelineCommand());

        meterProvider.ForceFlush();

        exportedMetrics.ShouldContain(m => m.Name == "herrgeneral.commands.total");
        exportedMetrics.ShouldContain(m => m.Name == "herrgeneral.commands.duration");
        exportedMetrics.ShouldContain(m => m.Name == "herrgeneral.events.total");
        exportedMetrics.ShouldContain(m => m.Name == "herrgeneral.events.duration");
    }

    [Fact]
    public void SupportFluentChainingOnBuilders()
    {
        var tracerBuilder = Sdk.CreateTracerProviderBuilder();
        var chainedTracerBuilder = tracerBuilder.AddHerrGeneralInstrumentation();
        chainedTracerBuilder.ShouldBeSameAs(tracerBuilder);

        var meterBuilder = Sdk.CreateMeterProviderBuilder();
        var chainedMeterBuilder = meterBuilder.AddHerrGeneralInstrumentation();
        chainedMeterBuilder.ShouldBeSameAs(meterBuilder);
    }

    [Fact]
    public void ThrowArgumentNullExceptionWhenTracerProviderBuilderIsNull()
    {
        TracerProviderBuilder? builder = null;
        Should.Throw<ArgumentNullException>(() => builder!.AddHerrGeneralInstrumentation());
    }

    [Fact]
    public void ThrowArgumentNullExceptionWhenMeterProviderBuilderIsNull()
    {
        MeterProviderBuilder? builder = null;
        Should.Throw<ArgumentNullException>(() => builder!.AddHerrGeneralInstrumentation());
    }
}
