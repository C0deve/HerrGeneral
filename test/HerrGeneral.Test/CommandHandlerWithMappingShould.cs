using HerrGeneral.Core;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class CommandHandlerWithMappingShould
{
    [Fact]
    public void ReturnEvents()
    {
        var mappers = new CommandHandlerMappings();
        mappers.AddMapping<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(x => x.Events);

        var sut = new CommandHandlerWithMapping<Ping, Ping.Handler, Unit>(new Ping.Handler(new EventTracker()), mappers);

        sut.Handle(new Ping())
            .Events
            .Count()
            .ShouldBe(1);
    }

    [Fact]
    public void ReturnEventsWhenMappingBaseClass()
    {
        var mappers = new CommandHandlerMappings();
        mappers.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase, Unit>,
            MyResult<Unit>>(x => x.Events);

        var sut = new CommandHandlerWithMapping<Ping, Ping.Handler, Unit>(new Ping.Handler(new EventTracker()), mappers);

        sut.Handle(new Ping())
            .Events
            .Count()
            .ShouldBe(1);
    }

    [Fact]
    public void ReturnValue()
    {
        var mappers = new CommandHandlerMappings();
        
        mappers.AddMapping<
            CommandBase,
            ILocalCommandHandler<CommandBase, int>,
            MyResult<int>,
            int>(
            x => x.Events,
            x => x.Result);

        var sut = new CommandHandlerWithMapping<PingWithReturnValue, PingWithReturnValue.Handler, int>(new PingWithReturnValue.Handler(), mappers);

        sut.Handle(new PingWithReturnValue())
            .Result
            .ShouldBe(1);
    }
}