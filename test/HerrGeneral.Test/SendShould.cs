using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendShould
{
    private readonly Mediator _mediator;
    private readonly ServiceProvider _serviceProvider;

    public SendShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<ReadModel>()
            .AddSingleton<ReadModelWithMultipleHandlers>()
            .AddSingleton<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .AddSingleton<Dependency>()
            .AddSingleton<Dependency2>()
            .UseHerrGeneral(configuration =>
                configuration
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase, Guid>, MyResult<Guid>, Guid>(result => result.Events, x => x.Result)
                    .MapEventHandlerOnWriteSide<EventBase, Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!));
        
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Resolve_main_handler() =>
        await new Ping { Message = "Ping" }
            .SendFrom(_mediator)
            .ShouldSuccess();

    [Fact]
    public async Task Resolve_handler_inheriting_from_ICommandHandler() =>
        await new PingWithDependenceOnHerrGeneral()
            .SendFrom(_mediator)
            .ShouldSuccess();

    private static readonly Guid AggregateId = Guid.NewGuid();

    [Fact]
    public async Task Resolve_main_handler_for_creation_command() =>
        await new CreatePing { AggregateId = AggregateId, Message = "Ping" }
            .SendFrom<Guid>(_mediator)
            .ShouldSuccessWithValue(AggregateId);

    [Fact]
    public async Task Dispatch_events_on_write_side()
    {
        await new Ping { Message = "Ping" }
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<Dependency>()
            .Called
            .ShouldBeTrue();
    }

    [Fact]
    public async Task Dispatch_events_on_read_side()
    {
        await new Ping { Message = "Ping" }
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<ReadModel>()
            .Message
            .ShouldBe("Ping received");
    }

    [Fact]
    public async Task Dispatch_events_on_write_side_when_handler_inheriting_IEventHandler()
    {
        await new Ping { Message = "Ping" }
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<Dependency2>()
            .Called
            .ShouldBeTrue();
    }
    
    [Fact]
    public async Task Dispatch_events_on_read_side_when_handler_inheriting_IEventHandler()
    {
        await new Ping { Message = "Ping" }
            .AssertSendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .Message
            .ShouldBe("Ping received");
    }

    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error()
    {
        await new PingWithFailureInCommandHandler()
            .SendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<ReadModel>()
            .Message
            .ShouldBe("");
    }

    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error_throw_from_event_handler()
    {
        await new PingWithFailureInEventHandler()
            .SendFrom(_mediator);

        _serviceProvider
            .GetRequiredService<ReadModel>()
            .Message
            .ShouldBe("");
    }
}