using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using HerrGeneral.Core.Diagnostics;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Tracing;

public class TracingShould(ITestOutputHelper output)
{
    private record FailingCommand
    {
        public class Handler : ICommandHandler<FailingCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(FailingCommand command)
                => throw new InvalidOperationException("Command processing failed intentionally.");
        }
    }

    /// <summary>
    /// Ensures that setting EnableCommandExecutionTracing to false prevents logging of command execution details.
    /// </summary>
    [Fact]
    public async Task NotTraceWhenDisabled()
    {
        var services = new ServiceCollection()
            .AddSingleton<ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(PingWithDependenceOnHerrGeneral).Namespace!)
                    .ScanReadSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler).Namespace!)
                    .EnableCommandExecutionTracing(false));

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<Mediator>().Send(new PingWithDependenceOnHerrGeneral()).ShouldSuccess();
    }

    /// <summary>
    /// Ensures that executing a command creates the proper ActivitySource spans with expected tags.
    /// </summary>
    [Fact]
    public async Task EmitOpenTelemetryActivities()
    {
        var stoppedActivities = new ConcurrentBag<Activity>();

        using var listener = new ActivityListener();
        listener.ShouldListenTo = source => source.Name == HerrGeneralDiagnostics.SourceName;
        listener.Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded;
        listener.ActivityStopped = stoppedActivities.Add;
        ActivitySource.AddActivityListener(listener);

        var services = new ServiceCollection()
            .AddSingleton<ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(PingWithDependenceOnHerrGeneral).Namespace!)
                    .ScanReadSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<Mediator>().Send(new PingWithDependenceOnHerrGeneral()).ShouldSuccess();

        var rootActivity = stoppedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.ExecuteCommand &&
            a.GetTagItem(HerrGeneralDiagnostics.Tags.CommandName)?.ToString() == "PingWithDependenceOnHerrGeneral");

        rootActivity.ShouldNotBeNull();
        rootActivity.Status.ShouldBe(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// Ensures that executing a command records metrics (CommandsTotal, CommandsDuration).
    /// </summary>
    [Fact]
    public async Task RecordOpenTelemetryMetrics()
    {
        var commandCountRecorded = 0L;
        var durationRecorded = false;
        var syncLock = new object();

        using var meterListener = new MeterListener();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == HerrGeneralDiagnostics.MeterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        meterListener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name != "herrgeneral.commands.total") return;
            lock (syncLock)
            {
                commandCountRecorded += measurement;
            }
        });

        meterListener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
        {
            if (instrument.Name != "herrgeneral.commands.duration") return;
            if (!(measurement > 0)) return;
            lock (syncLock)
            {
                durationRecorded = true;
            }
        });

        meterListener.Start();

        var services = new ServiceCollection()
            .AddSingleton<ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(PingWithDependenceOnHerrGeneral).Namespace!)
                    .ScanReadSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<Mediator>().Send(new PingWithDependenceOnHerrGeneral()).ShouldSuccess();

        lock (syncLock)
        {
            commandCountRecorded.ShouldBeGreaterThan(0);
            durationRecorded.ShouldBeTrue();
        }
    }

    /// <summary>
    /// Ensures that failing commands record exception and error status on Activity.
    /// </summary>
    [Fact]
    public async Task RecordExceptionOnActivityWhenFailureOccurs()
    {
        var stoppedActivities = new ConcurrentBag<Activity>();

        using var listener = new ActivityListener();
        listener.ShouldListenTo = source => source.Name == HerrGeneralDiagnostics.SourceName;
        listener.Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded;
        listener.ActivityStopped = stoppedActivities.Add;
        ActivitySource.AddActivityListener(listener);

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(TracingShould).Assembly, typeof(TracingShould).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var result = await mediator.Send(new FailingCommand());
        result.IsPanicError.ShouldBeTrue();

        var commandActivity = stoppedActivities.FirstOrDefault(a =>
            a.OperationName == HerrGeneralDiagnostics.Activities.ExecuteCommand &&
            a.GetTagItem(HerrGeneralDiagnostics.Tags.CommandName)?.ToString() == "FailingCommand");

        commandActivity.ShouldNotBeNull();
        commandActivity.Status.ShouldBe(ActivityStatusCode.Error);
        commandActivity.Events.ShouldContain(e => e.Name == "exception");
    }
}
