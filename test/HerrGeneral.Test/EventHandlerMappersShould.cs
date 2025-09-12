using HerrGeneral.Core.Configuration;
using HerrGeneral.Core.Error;
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
            var sut = new EventHandlerMappingsConfiguration();
            sut.GetFromEventType(typeof(Pong));
        });

    [Fact]
    public void ThrowExceptionWhenHandlerInterfaceIsNotGeneric() =>
        Should.Throw<HandlerTypeMustBeGenericMappingDefinitionException>(() =>
        {
            var sut = new EventHandlerMappingsConfiguration();
            sut.AddWriteSideMapping<object, object, bool>(_ => []);
        });

    [Fact]
    public void ReturnMapperFromEventType()
    {
        var sut = new EventHandlerMappingsConfiguration();
        sut.AddWriteSideMapping<EventBase, IEventHandler<EventBase>>();

        sut
            .GetFromEventType(typeof(Pong))
            .ShouldNotBeNull();
    }

    [Fact]
    public void ThrowExceptionWhenHandlerDoesNotReturnIEnumerableObject()
    {
        var sut = new EventHandlerMappingsConfiguration();

        Should.Throw<InvalidOperationException>(() => 
                sut.AddWriteSideMapping<Pong, IInvalidReturnTypeHandler<Pong>>())
            .Message
            .ShouldContain("must return a type that implements IEnumerable<object>");
    }
}

// Handler interface for test with invalid return type
public interface IInvalidReturnTypeHandler<in T> where T : notnull
{
    string Handle(T evt); // Returns a string instead of IEnumerable<object>
}