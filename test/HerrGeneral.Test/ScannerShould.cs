using HerrGeneral.Registration;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Scanner.Test;

public class ScannerShould
{
    [Fact]
    public void Get_all_command_handlers() =>
        new Registration.Scanner()
            .OnWriteSide(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .CommandHandlerTypes.ShouldBe(new[] { typeof(Command2Handler), typeof(Command1.Command1Handler) });

    [Fact]
    public void Get_all_command_handlers_without_namespace_filter() =>
        new Registration.Scanner()
            .OnWriteSide(typeof(Command1).Assembly)
            .Scan()
            .CommandHandlerTypes
            .ShouldNotBeEmpty();

    [Fact]
    public void Get_all_event_handlers() =>
        new Registration.Scanner()
            .OnWriteSide(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .EventHandlerTypes.ShouldBe(new[] { typeof(MyEventHandler), typeof(MyEventHandlerImpl) });
    
    [Fact]
    public void Get_all_read_event_handlers() =>
        new Registration.Scanner()
            .OnReadSide(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .ReadSideEventHandlerTypes.ShouldBe(new[] { typeof(MyReadSideEventHandlerImpl) });

    [Fact]
    public void Filter_ICommandHandler() =>
        typeof(Command1.Command1Handler).IsAssignableFromOpenType(typeof(ICommandHandler<,>)).ShouldBe(true);

    [Fact]
    public void Filter_IEventHandler() =>
        typeof(MyEventHandlerImpl).IsAssignableFromOpenType(typeof(IEventHandler<>)).ShouldBe(true);

    [Fact]
    public void Filter_IReadSideEventHandler() =>
        typeof(MyReadSideEventHandlerImpl).IsAssignableFromOpenType(typeof(Contracts.ReadSIde.IEventHandler<>)).ShouldBe(true);

    public class Command1 : CommandBase<CommandResultV2>
    {
        public class Command1Handler : CommandHandler<Command1>
        {
            public Command1Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
            {
            }

            public override Task<CommandResultV2> Handle(Command1 command, CancellationToken cancellationToken) =>
                Task.FromResult(CommandResultV2.Success);
        }
    }

    private class Command2 : CommandBase<CommandResultV2>
    {
    }

    private abstract class Command2HandlerBase : CommandHandler<Command2>
    {
        protected Command2HandlerBase(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override Task<CommandResultV2> Handle(Command2 command, CancellationToken cancellationToken) => 
            Task.FromResult(CommandResultV2.Success);
    }

    private class Command2Handler : Command2HandlerBase
    {
        public Command2Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }
    }

    private class MyEvent : EventBase
    {
        public MyEvent(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId, DateTime.Now)
        {
        }
    }

    private class MyEventHandler : IEventHandler<MyEvent>
    {
        public Task Handle(MyEvent notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private abstract class MyEventHandlerAbstract : IEventHandler<MyEvent>
    {
        public Task Handle(MyEvent notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private class MyEventHandlerImpl : MyEventHandlerAbstract
    {
    }

    private class MyReadSideEventHandlerImpl : Contracts.ReadSIde.IEventHandler<MyEvent>
    {
        public Task Handle(MyEvent notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}