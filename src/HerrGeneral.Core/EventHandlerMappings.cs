using System.Collections.Concurrent;
using System.Reflection;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;

namespace HerrGeneral.Core;

/// <summary>
/// Provides a mechanism to map client event handlers to internal event handlers.
/// <para>
/// This class serves as the bridge between external handler implementations and the internal event handling system.
/// It resolves handler methods at runtime and manages a cache to improve performance when the same handler
/// is used multiple times.
/// </para>
/// </summary>
internal class EventHandlerMappings(Registration.EventHandlerMappings mappings) : IReadSideEventHandlerMappings, IWriteSideEventHandlerMappings
{
    // Cache for handler methods to avoid expensive reflection lookups
    private readonly ConcurrentDictionary<(Type EventType, Type HandlerType), MethodInfo> _handleMethodCache = new();

    #region Method Resolution

    /// <summary>
    /// Resolves and returns the method that should handle a specific event type in a given handler class.
    /// </summary>
    /// <param name="evtType">The type of event to be handled</param>
    /// <param name="handlerType">The type of handler class that will process the event</param>
    /// <returns>A MethodInfo object representing the handler method to be invoked</returns>
    /// <remarks>
    /// This method implements caching to avoid repeated expensive reflection operations.
    /// It first checks an internal cache, and if not found, it resolves the method using reflection.
    /// </remarks>
    public MethodInfo GetHandleMethod(Type evtType, Type handlerType) => 
        GetCachedOrResolveMethod(evtType, handlerType).Method;

    /// <summary>
    /// Implementation of IWriteSideEventHandlerMappings.GetHandleMethod that returns both the handler method
    /// and its associated event mapping configuration.
    /// </summary>
    /// <param name="evtType">The type of event to be handled</param>
    /// <param name="handlerType">The type of handler class that will process the event</param>
    /// <returns>
    /// A tuple containing:
    /// - Method: The MethodInfo for the handler method
    /// - Mapping: The EventHandlerMapping containing configuration for handling this event type
    /// </returns>
    /// <remarks>
    /// This extended version is used by write-side handlers that need additional mapping information
    /// to process events and their results correctly.
    /// </remarks>
    (MethodInfo Method, EventHandlerMapping Mapping) IWriteSideEventHandlerMappings.GetHandleMethod(Type evtType, Type handlerType) => 
        GetCachedOrResolveMethod(evtType, handlerType);

    /// <summary>
    /// Core implementation that retrieves handler method information from cache or resolves it using reflection.
    /// </summary>
    /// <param name="evtType">The type of event to be handled</param>
    /// <param name="handlerType">The type of handler class that will process the event</param>
    /// <returns>A tuple containing the handler method and its associated mapping configuration</returns>
    /// <remarks>
    /// This method implements the following strategy:
    /// 1. First, it retrieves the event type mapping configuration
    /// 2. It checks if the method information is available in the cache
    /// 3. If not found, it performs reflection to find the appropriate method
    /// 4. The result is cached for future use to improve performance
    /// </remarks>
    private (MethodInfo Method, EventHandlerMapping Mapping) GetCachedOrResolveMethod(Type evtType, Type handlerType)
    {
        // Get the mapping for this event type
        var mapping = mappings.GetFromEventType(evtType);

        // Check cache first to avoid expensive reflection
        var cacheKey = (EventType: evtType, HandlerType: handlerType);
        if (_handleMethodCache.TryGetValue(cacheKey, out var cachedMethodInfo))
            return (cachedMethodInfo, mapping);

        // Find the matching method in the handler type
        var methodInfo = FindMatchingHandleMethod(handlerType, mapping.MethodInfo.Name, evtType);

        // Cache the result for future calls
        _handleMethodCache.TryAdd(cacheKey, methodInfo);

        return (methodInfo, mapping);
    }

    /// <summary>
    /// Locates the specific method in a handler class that should process a given event type.
    /// </summary>
    /// <param name="handlerType">The type of handler class to search within</param>
    /// <param name="methodName">The expected name of the handler method</param>
    /// <param name="eventType">The type of event that the method should accept</param>
    /// <returns>The MethodInfo object for the matching handler method</returns>
    /// <exception cref="HandleMethodNotFoundException">Thrown when no matching method is found in the handler type</exception>
    /// <remarks>
    /// A matching method must:
    /// - Have the exact name specified in methodName
    /// - Accept exactly one parameter of the specified eventType
    /// </remarks>
    private static MethodInfo FindMatchingHandleMethod(Type handlerType, string methodName, Type eventType)
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

    #endregion
}