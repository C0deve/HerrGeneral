using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;

namespace HerrGeneral.Core;

/// <summary>
/// Responsible for registering mappings between event types and their handlers
/// </summary>
internal class EventHandlerMappingRegistration
{
    private readonly Dictionary<Type, EventHandlerMapping> _handlerMappers = new();
    
    #region Event Mapping Lookup

    /// <summary>
    /// Finds and returns the handler mapping for a given event type
    /// </summary>
    /// <param name="evtType">Type of the event to find a mapping for</param>
    /// <returns>The corresponding EventHandlerMapping</returns>
    /// <exception cref="MissingEventHandlerMapperException">Thrown when no mapping is found</exception>
    public EventHandlerMapping GetFromEventType(Type evtType)
    {
        // Search in reverse order to find the most specific match
        foreach (var key in _handlerMappers.Keys.Reverse())
        {
            if (evtType.IsAssignableTo(key))
                return _handlerMappers[key];
        }

        throw new MissingEventHandlerMapperException(evtType);
    }

    /// <summary>
    /// Returns all registered handler mappings
    /// </summary>
    public IEnumerable<EventHandlerMapping> All() => _handlerMappers.Values;

    #endregion
    
    /// <summary>
    /// Adds a mapping for an event type to a handler with a return value conversion
    /// </summary>
    public EventHandlerMappingRegistration AddMapping<TEvent, THandler, THandlerReturn>(
        Func<THandlerReturn, IEnumerable<object>> mapEvents)
    {
        var handlerType = typeof(THandler);

        // Find a method that takes a parameter of type TEvent
        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TEvent))
            ?? throw new NotSupportedException($"No method that accepts a unique parameter of type '{typeof(TEvent)}' was found on type '{typeof(THandler).Name}'");

        // Verify the method's return type matches THandlerReturn
        var returnType = methodInfo.ReturnType;
        if (returnType != typeof(THandlerReturn))
            throw new TypeMismatchInMappingDefinitionException(typeof(THandlerReturn), methodInfo);

        // Ensure the handler type is generic
        if (!handlerType.IsGenericType)
            throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);

        // Register the mapping
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

    /// <summary>
    /// Adds a mapping for an event type to a handler without return value conversion
    /// </summary>
    public EventHandlerMappingRegistration AddMapping<TEvent, THandler>()
    {
        var handlerType = typeof(THandler);

        // Find a method that takes a parameter of type TEvent
        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TEvent))
            ?? throw new NotSupportedException($"No method that accepts a unique parameter of type '{typeof(TEvent)}' was found on type '{typeof(THandler).Name}'");

        // Ensure the handler type is generic
        if (!handlerType.IsGenericType)
            throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);

        // Register the mapping
        _handlerMappers.Add(
            typeof(TEvent),
            new EventHandlerMapping(
                methodInfo,
                handlerType.GetGenericTypeDefinition(),
                null)
        );

        return this;
    }
}
