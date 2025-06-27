using HerrGeneral.Core;
using HerrGeneral.Core.Error;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WriteSide;
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
            var sut = new EventHandlerMappings();
            sut.GetFromEvent(new Pong("", Guid.NewGuid(), Guid.NewGuid()));
        });

    [Fact]
    public void ThrowExceptionWhenHandlerInterfaceIsNotGeneric() =>
        Should.Throw<HandlerTypeMustBeGenericMappingDefinitionException>(() =>
        {
            var sut = new EventHandlerMappings();
            sut.AddMapping<object, object, bool>(_ => []);
        });

    [Fact]
    public void ReturnMapperFroEvent()
    {
        var sut = new EventHandlerMappings();
        sut.AddMapping<EventBase, IEventHandler<EventBase>>();

        sut
            .GetFromEvent(new Pong("", Guid.NewGuid(), Guid.NewGuid()))
            .ShouldNotBeNull();
    }
}