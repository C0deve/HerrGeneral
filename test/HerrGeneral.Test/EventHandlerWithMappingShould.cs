using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class EventHandlerWithMappingShould
{
    [Fact]
    public void HandleEvent()
    {
        var mappers = new EventHandlerMappingRegistration();
        mappers.AddMapping<EventBase, IEventHandler<EventBase>>();
        var dependency = new CommandTracker2();
        var sut = new EventHandlerWithMapping<Pong, PongHandler>(new PongHandler(dependency), new EventHandlerMappings(mappers));

        var sourceCommandId = Guid.NewGuid();
        sut.Handle(new Pong(sourceCommandId, Guid.NewGuid()));
            
        dependency
            .HasHandled(sourceCommandId)
            .ShouldBeTrue();
    }
}