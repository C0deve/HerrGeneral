using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WriteSide;
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
        var dependency = new Dependency();
        var sut = new EventHandlerWithMapping<Pong, PongMiddleHandler>(new PongMiddleHandler(dependency), new EventHandlerMappings(mappers));

        sut.Handle(new Pong("", Guid.NewGuid(), Guid.NewGuid()));
            
        dependency.Called.ShouldBeTrue();
    }
}