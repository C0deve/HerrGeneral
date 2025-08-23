using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WithMapping.ReadSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendWithMappingShould
{
    private readonly Mediator _mediator;
    private readonly ServiceProvider _serviceProvider;

    public SendWithMappingShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<EventTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .RegisterCommandHandlerWithMapping<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .RegisterCommandHandlerWithMapping<CommandBase, ILocalCommandHandler<CommandBase, Guid>, MyResult<Guid>, Guid>(it => it.Events, it => it.Result)
                    .RegisterWriteSideEventHandlerWithMapping<EventBase, Test.Data.WithMapping.WriteSide.ILocalEventHandler<EventBase>, MyEventHandlerResult>(x => x.Events)
                    .RegisterReadSideEventHandler<EventBase, Test.Data.WithMapping.ReadSide.ILocalEventHandler<EventBase>>()
                    .ScanWriteSideOn(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .ScanReadSideOn(typeof(Ping).Assembly, typeof(AReadModel).Namespace!));

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Resolve_main_handler() =>
        await new Ping().SendFrom(_mediator)
            .ShouldSuccess();

    [Fact]
    public async Task Manage_parallel_processing()
    {
        var t1 = new Ping().SendFrom(_mediator)
            .ShouldSuccess();
        
        var t2 = new Ping().SendFrom(_mediator)
            .ShouldSuccess();
        
        var t3 = new Ping().SendFrom(_mediator)
            .ShouldSuccess();
        
        await Task.WhenAll(t1, t2, t3);
    }


    private static readonly Guid AggregateId = Guid.NewGuid();

    [Fact]
    public async Task Resolve_main_handler_for_creation_command() =>
        await new CreatePing { AggregateId = AggregateId, Message = "Ping" }
            .SendFrom<Guid>(_mediator)
            .ShouldSuccessWithValue(AggregateId);
    
    
    [Fact]
    public async Task Dispatch_events_on_write_side()
    {
        var ping = new Ping();
        await ping
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<EventTracker>()
            .GetEventsWithSourceCommandId(ping.Id)
            .Select(x => x.GetType().Name)
            .ShouldBe([nameof(Pong), nameof(PongPong), nameof(PongPongPong)]);
    }

    [Fact]
    public async Task Dispatch_events_on_read_side()
    {
        var ping = new Ping();

        await ping
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<AReadModel>()
            .GetEventsWithSourceCommandId(ping.Id)
            .Select(x => x.GetType().Name)
            .ShouldBe([nameof(Pong), nameof(PongPong), nameof(PongPongPong)]);
    }


    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error()
    {
        var command = new PingWithFailureInCommandHandler();
        await command
            .SendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<AReadModel>()
            .GetEventsWithSourceCommandId(command.Id)
            .ShouldBeEmpty();
    }

    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error_throw_from_event_handler()
    {
        var command = new PingWithFailureInEventHandler();
        await command
            .SendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<AReadModel>()
            .GetEventsWithSourceCommandId(command.Id)
            .ShouldBeEmpty();
    }
}