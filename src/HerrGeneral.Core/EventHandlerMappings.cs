using System.Reflection;
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

    private readonly Dictionary<(Type, Type), MethodInfo> _handleMethodCache = new();

    /// <summary>
    /// Return handlerMapper corresponding to <see cref="evtType"/>
    /// </summary>
    /// <param name="evtType"></param>
    /// <returns></returns>
    public EventHandlerMapping GetFromEventType(Type evtType)
    {
        foreach (var key in _handlerMappers.Keys.Reverse())
        {
            if (evtType.IsAssignableTo(key))
                return _handlerMappers[key];
        }

        throw new MissingEventHandlerMapperException(evtType);
    }
    
    /// <summary>
    /// Return <see cref="MethodInfo"/> associated with event type and handler type
    /// </summary>
    /// <param name="evtType">Type of the event</param>
    /// <param name="handlerType">Type of the handler</param>
    /// <returns>MethodInfo for the handler method</returns>
    /// <exception cref="HandleMethodNotFoundException">Thrown when no matching method is found</exception>
    public MethodInfo GetHandleMethod(Type evtType, Type handlerType)
    {
        // Check cache first to avoid expensive reflection
        var cacheKey = (evtType, handlerType);
        if (_handleMethodCache.TryGetValue(cacheKey, out var cachedMethodInfo))
            return cachedMethodInfo;

        // Get the mapping for this event type
        var mapping = GetFromEventType(evtType);

        // Find the matching method in the handler type
        var methodInfo = FindMatchingHandleMethod(handlerType, mapping.MethodInfo.Name, evtType);

        // Cache the result for future calls
        _handleMethodCache.Add(cacheKey, methodInfo);
        
        return methodInfo;
    }

    /// <summary>
    /// Finds the matching handler method in the handler type
    /// </summary>
    /// <param name="handlerType">Type of the handler</param>
    /// <param name="methodName">Name of the method to find</param>
    /// <param name="eventType">Type of the event</param>
    /// <returns>MethodInfo of the found method</returns>
    /// <exception cref="HandleMethodNotFoundException">Thrown if no matching method is found</exception>
    private MethodInfo FindMatchingHandleMethod(Type handlerType, string methodName, Type eventType)
    {
        var matchingMethod = handlerType
            .GetMethods()
            .SingleOrDefault(method => 
                method.Name == methodName && 
                method.HasUniqueParameterOfType(eventType));

        if (matchingMethod == null)
            throw new HandleMethodNotFoundException(eventType, handlerType, methodName);

        return matchingMethod;
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