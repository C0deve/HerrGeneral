using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class EventHandlerMappersShould
{
    [Fact]
    public void ThrowExceptionWhenNoHandlerRegistered() =>
        Should.Throw<MissingEventHandlerMapperException>(() =>
        {
            var sut = new EventHandlerMappingRegistration();
            sut.GetFromEventType(typeof(Pong));
        });

    [Fact]
    public void ThrowExceptionWhenHandlerInterfaceIsNotGeneric() =>
        Should.Throw<HandlerTypeMustBeGenericMappingDefinitionException>(() =>
        {
            var sut = new EventHandlerMappingRegistration();
            sut.AddMapping<object, object, bool>(_ => []);
        });

    [Fact]
    public void ReturnMapperFroEvent()
    {
        var sut = new EventHandlerMappingRegistration();
        sut.AddMapping<EventBase, IEventHandler<EventBase>>();

        sut
            .GetFromEventType(typeof(Pong))
            .ShouldNotBeNull();
    }
}