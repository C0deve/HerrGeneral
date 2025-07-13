using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendShould
{
    private readonly Mediator _mediator;
    private readonly Container _container;

    public SendShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);

            cfg.ForSingletonOf<ReadModel>().Use<ReadModel>();
            cfg.ForSingletonOf<Dependency>().Use<Dependency>();

            cfg.UseHerrGeneral(configuration =>
                configuration
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase, Guid>, MyResult<Guid>, Guid>(result => result.Events, x => x.Result)
                    .MapEventHandlerOnWriteSide<EventBase, Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!));
        });
        _mediator = _container.GetInstance<Mediator>();
    }

    [Fact]
    public async Task Resolve_main_handler() =>
        await new Ping { Message = "Ping" }
            .SendFromMediator(_mediator)
            .ShouldSuccess();

    private static readonly Guid AggregateId = Guid.NewGuid();

    [Fact]
    public async Task Resolve_main_handler_for_creation_command() =>
        await new CreatePing { AggregateId = AggregateId, Message = "Ping" }
            .SendFromMediator<Guid>(_mediator)
            .ShouldSuccessAndReturnValue(AggregateId);

    [Fact]
    public async Task Dispatch_events_on_write_side()
    {
        await new Ping { Message = "Ping" }
            .SendFromMediator(_mediator)
            .ShouldSuccess();

        _container.GetInstance<Dependency>().Called.ShouldBeTrue();
    }

    [Fact]
    public async Task Dispatch_events_on_read_side()
    {
        await new Ping { Message = "Ping" }
            .SendFromMediator(_mediator)
            .ShouldSuccess();

        _container.GetInstance<ReadModel>().Message.ShouldBe("Ping received");
    }

    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error()
    {
        await new PingWithFailureInCommandHandler().SendFromMediator(_mediator);

        _container.GetInstance<ReadModel>().Message.ShouldBe("");
    }

    [Fact]
    public async Task Not_dispatch_events_on_read_side_on_domain_error_throw_from_event_handler()
    {
        await new PingWithFailureInEventHandler().SendFromMediator(_mediator);

        _container.GetInstance<ReadModel>().Message.ShouldBe("");
    }
}