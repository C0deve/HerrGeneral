using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Tracing;

public class TracingShould(ITestOutputHelper output)
{
    /// <summary>
    /// Ensures that setting EnableCommandExecutionTracing to false prevents logging of command execution details.
    /// </summary>
    [Fact]
    public async Task NotTraceWhenDisabled()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(PingWithDependenceOnHerrGeneral).Namespace!)
                    .ScanReadSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(ReadModelWithMultipleHandlersAndInheritingIEventHandler).Namespace!)
                    .EnableCommandExecutionTracing(false));

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<Mediator>().Send(new PingWithDependenceOnHerrGeneral()).ShouldSuccess();
    }
}