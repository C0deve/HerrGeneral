using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class EventHandlerWithMappingShould
{
    [Fact]
    public void HandleEvent()
    {
        var mappers = new EventHandlerMappingRegistration();
        mappers.AddWriteSideMapping<EventBase, ILocalEventHandler<EventBase>, MyEventHandlerResult>(x => x.Events);
        
        var commandTracker2 = new CommandTracker2();
        var sut = new EventHandlerWithMapping<Pong, PongHandler>(new PongHandler(commandTracker2,
                new EventTracker()),
            new EventHandlerMappings(mappers));

        var sourceCommandId = Guid.NewGuid();
        sut.Handle(new Pong(sourceCommandId, Guid.NewGuid()));

        commandTracker2
            .HasHandled(sourceCommandId)
            .ShouldBeTrue();
    }
}