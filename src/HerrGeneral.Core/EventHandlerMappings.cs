using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core;

/// <summary>
/// Register mapping between client event handler and internal <see cref="IEventHandler{TEvent}"/>*
/// Used by internal <see cref="EventHandler"/> to return <see cref="IEnumerable{Object}"/> from a client handler
/// </summary>
internal class EventHandlerMappings : IReadSideEventHandlerMappings, IWriteSideEventHandlerMappings
{
    /// <summary>
    /// Map event type with an <see cref="EventHandlerMapping"/>
    /// </summary>
    private readonly Dictionary<Type, EventHandlerMapping> _handlerMappers = new();

    /// <summary>
    /// Return handlerMapper corresponding to <see cref="@event"/>
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    public EventHandlerMapping GetFromEvent(object @event)
    {
        foreach (var key in _handlerMappers.Keys.Reverse())
        {
            if (@event.GetType().IsAssignableTo(key))
                return _handlerMappers[key];
        }

        throw new MissingEventHandlerMapperException(@event.GetType());
    }

    public EventHandlerMappings AddMapping<TEvent, THandler, THandlerReturn>(
        Func<THandlerReturn, IEnumerable<object>> mapEvents)
    {
        var handlerType = typeof(THandler);

        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TEvent))
                         ?? throw new NotSupportedException($"No method that accepts a unique parameter of type '{typeof(TEvent)}' was found on type '{typeof(THandler).Name}'");

        var returnType = methodInfo.ReturnType;
        if (returnType != typeof(THandlerReturn))
            throw new TypeMismatchInMappingDefinitionException(typeof(THandlerReturn), methodInfo);

        if (!handlerType.IsGenericType)
            throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);


        _handlerMappers.Add(
            typeof(TEvent),
            new EventHandlerMapping(
                methodInfo,
                handlerType.GetGenericTypeDefinition(),
                o => mapEvents((THandlerReturn)o)
            )
        );

        return this;
    }

    public EventHandlerMappings AddMapping<TEvent, THandler>()
    {
        var handlerType = typeof(THandler);

        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TEvent))
                         ?? throw new NotSupportedException($"No method that accepts a unique parameter of type '{typeof(TEvent)}' was found on type '{typeof(THandler).Name}'");

        if (!handlerType.IsGenericType)
            throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);

        _handlerMappers.Add(
            typeof(TEvent),
            new EventHandlerMapping(
                methodInfo,
                handlerType.GetGenericTypeDefinition(),
                null)
        );

        return this;
    }
    
    public IEnumerable<EventHandlerMapping> All() => _handlerMappers.Values;
}