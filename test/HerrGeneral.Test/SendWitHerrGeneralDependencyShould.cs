using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendWitHerrGeneralDependencyShould
{
    private readonly Mediator _mediator;
    private readonly ServiceProvider _serviceProvider;

    public SendWitHerrGeneralDependencyShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddSingleton<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(PingWithDependenceOnHerrGeneral).Namespace!)
                    .ScanReadSideOn(typeof(PingWithDependenceOnHerrGeneral).Assembly, typeof(ReadModelWithMultipleHandlersAndInheritingIEventHandler).Namespace!));

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Resolve_handler() =>
        await new PingWithDependenceOnHerrGeneral()
            .SendFrom(_mediator)
            .ShouldSuccess();

    [Fact]
    public async Task Dispatch_events_on_write_side()
    {
        var command = new PingWithDependenceOnHerrGeneral();

        await command
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<EventTracker>()
            .GetEventsWithSourceCommandId(command.Id)
            .Select(x => x.GetType().Name)
            .ShouldBe([nameof(Pong), nameof(PongPong)]);
    }

    [Fact]
    public async Task Dispatch_events_on_read_side()
    {
        var command = new PingWithDependenceOnHerrGeneral();
        await command
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .GetEventsWithSourceCommandId(command.Id)
            .Select(x => x.GetType().Name)
            .ShouldBe([nameof(Pong)]);
    }
}