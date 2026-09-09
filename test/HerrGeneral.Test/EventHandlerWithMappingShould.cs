using HerrGeneral.Core.Configuration;
using HerrGeneral.Core.WriteSide;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class EventHandlerWithMappingShould
{
    [Fact]
    public void HandleEvent()
    {
        var mappers = new EventHandlerMappingsConfiguration();
        mappers.AddWriteSideMapping<EventBase, HerrGeneral.Test.Data.WithMapping.WriteSide.ILocalEventHandler<EventBase>, MyEventHandlerResult>(x => x.Events);
        
        var eventTracker = new EventTracker();
        var sut = new EventHandlerWithMapping<Pong, PongHandler>(new PongHandler(eventTracker),
            new EventHandlerMappingsProvider(mappers));

        var sourceCommandId = Guid.NewGuid();
        sut.Handle(new Pong(sourceCommandId, Guid.NewGuid()));

        eventTracker
            .GetEventsWithSourceCommandId(sourceCommandId)
            .Count()
            .ShouldBe(1);
    }
}