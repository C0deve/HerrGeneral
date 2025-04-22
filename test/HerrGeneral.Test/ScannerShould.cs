using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Scanner.Test;

public class ScannerShould
{
    [Fact]
    public void Get_all_command_handlers() =>
        new Core.Registration.Scanner()
            .AddWriteSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .CommandHandlerWithReturnTypes.ShouldBe([typeof(Command2Handler), typeof(Command1.Command1Handler)]);

    [Fact]
    public void Get_all_command_handlers_without_namespace_filter() =>
        new Core.Registration.Scanner()
            .AddWriteSideAssembly(typeof(Command1).Assembly)
            .Scan()
            .CommandHandlerWithReturnTypes
            .ShouldNotBeEmpty();

    [Fact]
    public void Get_all_event_handlers() =>
        new Core.Registration.Scanner()
            .AddWriteSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .EventHandlerTypes.ShouldBe([typeof(MyEventHandler), typeof(MyEventHandlerImpl)]);

    [Fact]
    public void Get_all_read_event_handlers() =>
        new Core.Registration.Scanner()
            .AddReadSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!)
            .Scan()
            .ReadSideEventHandlerTypes.ShouldBe([typeof(MyReadSideEventHandlerImpl)]);

    [Fact]
    public void Filter_ICommandHandler() =>
        typeof(Command1.Command1Handler).IsAssignableFromOpenType(typeof(ICommandHandler<,>)).ShouldBe(true);

    [Fact]
    public void Filter_IEventHandler() =>
        typeof(MyEventHandlerImpl).IsAssignableFromOpenType(typeof(IEventHandler<>)).ShouldBe(true);

    [Fact]
    public void Filter_IReadSideEventHandler() =>
        typeof(MyReadSideEventHandlerImpl).IsAssignableFromOpenType(typeof(ReadSide.IEventHandler<>)).ShouldBe(true);

    public record Command1 : Change
    {
        public class Command1Handler : ChangeHandler<Command1>
        {
            public override (IEnumerable<object> Events, Unit Result) Handle(Command1 command, CancellationToken cancellationToken) =>
                ([], Unit.Default);
        }
    }

    private record Command2 : Change;

    private abstract class Command2HandlerBase : ChangeHandler<Command2>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(Command2 command, CancellationToken cancellationToken) =>
            ([], Unit.Default);
    }

    private class Command2Handler : Command2HandlerBase;

    private record MyEvent : EventBase
    {
        public MyEvent(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId, DateTime.Now)
        {
        }
    }

    private class MyEventHandler : IEventHandler<MyEvent>
    {
        public void Handle(MyEvent notification, CancellationToken cancellationToken)
        {
        }
    }

    private abstract class MyEventHandlerAbstract : IEventHandler<MyEvent>
    {
        public void Handle(MyEvent notification, CancellationToken cancellationToken)
        {
        }
    }

    private class MyEventHandlerImpl : MyEventHandlerAbstract;

    private class MyReadSideEventHandlerImpl : ReadSide.IEventHandler<MyEvent>
    {
        public void Handle(MyEvent notification, CancellationToken cancellationToken)
        {
        }
    }
}