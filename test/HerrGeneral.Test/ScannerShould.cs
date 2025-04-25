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
        new Core.Registration.Scanner(
                new Configuration()
                    .UseWriteSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!))
            .Scan()
            .CommandHandlerWithReturnTypes.ShouldBe([typeof(Command2Handler), typeof(Command1.Command1Handler)]);

    [Fact]
    public void Get_all_command_handlers_without_namespace_filter() =>
        new Core.Registration.Scanner(
                new Configuration()
                    .UseWriteSideAssembly(typeof(Command1).Assembly))
            .Scan()
            .CommandHandlerWithReturnTypes
            .ShouldNotBeEmpty();
    
    [Fact]
    public void Get_all_event_handlers() =>
        new Core.Registration.Scanner(
                new Configuration()
                    .UseWriteSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!))
            .Scan()
            .EventHandlerTypes.ShouldBe([typeof(MyEventHandler), typeof(MyEventHandlerImpl)]);

    [Fact]
    public void Get_all_read_event_handlers() =>
        new Core.Registration.Scanner(
                new Configuration()
                    .UseReadSideAssembly(typeof(Command1).Assembly, typeof(Command1).Namespace!))
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

    public record Command1
    {
        public class Command1Handler : CommandHandler<Command1>
        {
            protected override IEnumerable<object> InnerHandle(Command1 command) =>
                [];
        }
    }

    private record Command2;

    private abstract class Command2HandlerBase : CommandHandler<Command2>
    {
        protected override IEnumerable<object> InnerHandle(Command2 command) =>
            [];
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
        public void Handle(MyEvent notification)
        {
        }
    }

    private abstract class MyEventHandlerAbstract : IEventHandler<MyEvent>
    {
        public void Handle(MyEvent notification)
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