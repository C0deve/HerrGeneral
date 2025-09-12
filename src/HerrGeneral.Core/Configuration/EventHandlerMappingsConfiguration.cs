using System.Reflection;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;

namespace HerrGeneral.Core.Configuration;

/// <summary>
/// Provides a mechanism to register, manage, and retrieve mappings between event types and their corresponding handlers.
/// </summary>
internal class EventHandlerMappingsConfiguration
{
    private readonly Dictionary<Type, EventHandlerMapping> _handlerMappers = new();

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

    /// <summary>
    /// Adds a mapping for an event type to a handler with a return value conversion
    /// </summary>
    public EventHandlerMappingsConfiguration AddWriteSideMapping<TEvent, THandler, THandlerReturn>(
        Func<THandlerReturn, IEnumerable<object>> mapEvents)
    {
        var (methodInfo, handlerType) = ValidateHandlerAndGetMethod<TEvent, THandler>();

        ValidateReturnTypeMatches<THandlerReturn>(methodInfo);

        RegisterMapping<TEvent>(methodInfo, handlerType, o => mapEvents((THandlerReturn)o));

        return this;
    }

    /// <summary>
    /// Adds a mapping for an event type to a handler without return value conversion
    /// </summary>
    public EventHandlerMappingsConfiguration AddWriteSideMapping<TEvent, THandler>()
    {
        var (methodInfo, handlerType) = ValidateHandlerAndGetMethod<TEvent, THandler>();

        ValidateReturnTypeImplementsIEnumerable(methodInfo, handlerType);

        RegisterMapping<TEvent>(methodInfo, handlerType, null);

        return this;
    }

    /// <summary>
    /// Adds a mapping for an event type to a handler for read-side operations
    /// </summary>
    public EventHandlerMappingsConfiguration AddReadSideMapping<TEvent, THandler>()
    {
        var (methodInfo, handlerType) = ValidateHandlerAndGetMethod<TEvent, THandler>();

        RegisterMapping<TEvent>(methodInfo, handlerType, null);

        return this;
    }

    private static (MethodInfo methodInfo, Type handlerType) ValidateHandlerAndGetMethod<TEvent, THandler>()
    {
        var handlerType = typeof(THandler);

        // Find method that accepts the event type
        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TEvent))
                         ?? throw new NotSupportedException(
                             $"No method that accepts a unique parameter of type '{typeof(TEvent)}' " +
                             $"was found on type '{handlerType.Name}'");

        // Ensure the handler type is generic
        return handlerType.IsGenericType 
            ? (methodInfo, handlerType) 
            : throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);
    }

    private static void ValidateReturnTypeMatches<THandlerReturn>(MethodInfo methodInfo)
    {
        if (methodInfo.ReturnType != typeof(THandlerReturn))
            throw new TypeMismatchInMappingDefinitionException(typeof(THandlerReturn), methodInfo);
    }

    private static void ValidateReturnTypeImplementsIEnumerable(MethodInfo methodInfo, Type handlerType)
    {
        if (!typeof(IEnumerable<object>).IsAssignableFrom(methodInfo.ReturnType))
        {
            throw new InvalidOperationException(
                $"Method '{methodInfo.Name}' in handler type '{handlerType.Name}' must return a type " +
                $"that implements IEnumerable<object>. Current return type is '{methodInfo.ReturnType.Name}'. " +
                $"Either change the return type or use {nameof(ConfigurationBuilder.RegisterWriteSideEventHandlerWithMapping)} " +
                $"to provide a conversion function.");
        }
    }

    private void RegisterMapping<TEvent>(
        MethodInfo methodInfo, 
        Type handlerType, 
        Func<object, IEnumerable<object>>? eventMapper)
    {
        _handlerMappers.Add(
            typeof(TEvent),
            new EventHandlerMapping(
                methodInfo,
                handlerType.GetGenericTypeDefinition(),
                eventMapper)
        );
    }
}