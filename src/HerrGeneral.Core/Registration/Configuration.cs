using System.Reflection;
using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Configuration class for registering and managing command and event handlers.
/// This class centralizes the configuration of assemblies to scan for both read and write sides.
/// </summary>
public class Configuration
{
    private readonly HashSet<ScanParam> _writeSideSearchParams = [];
    private readonly HashSet<ScanParam> _readSideSearchParams = [];
    private readonly HashSet<Type> _domainExceptionInterfaces = [];

    /// <summary>
    /// Set of search parameters for write side assemblies.
    /// Contains the assemblies and namespaces to scan for discovering event and command handlers.
    /// </summary>
    internal IEnumerable<ScanParam> WriteSideSearchParams => _writeSideSearchParams.AsEnumerable();

    /// <summary>
    /// Set of search parameters for read side assemblies.
    /// Contains the assemblies and namespaces to scan for discovering read side event handlers.
    /// </summary>
    internal IEnumerable<ScanParam> ReadSideSearchParams => _readSideSearchParams.AsEnumerable();

    /// <summary>
    /// Set of registered domain exception types.
    /// These types are used to identify and handle exceptions specific to the business domain.
    /// </summary>
    internal IReadOnlySet<Type> DomainExceptionTypes => _domainExceptionInterfaces;

    /// <summary>
    /// Collection of mappings for external command handlers.
    /// These mappings allow integration of command handlers from external sources into the system.
    /// </summary>
    internal CommandHandlerMappings CommandHandlerMappings { get; } = new();
    
    /// <summary>
    /// Collection of mappings for external write side event handlers.
    /// Allows registration of event handlers that modify the system state.
    /// </summary>
    internal EventHandlerMappingRegistration WriteSideEventHandlerMappings { get; } = new();

    /// <summary>
    /// Collection of mappings for external read side event handlers.
    /// Allows registration of event handlers that update views and projections.
    /// </summary>
    internal EventHandlerMappingRegistration ReadSideEventHandlerMappings { get; } = new();
    
    internal Configuration()
    {
    }

    /// <summary>
    /// Adds an assembly to scan for the read side.
    /// Event handlers discovered in this assembly will be used to update views and projections.
    /// </summary>
    /// <param name="assembly">The assembly to scan for read side event handlers.</param>
    /// <param name="namespaces">Optional list of namespaces to limit the search. If not specified, the entire assembly will be scanned.</param>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration UseReadSideAssembly(Assembly assembly, params string[] namespaces)
    {
        _readSideSearchParams.Add(new ScanParam(assembly, namespaces));
        return this;
    }

    /// <summary>
    /// Adds an assembly to scan for the write side.
    /// Command and event handlers discovered in this assembly will be used to modify the system state.
    /// </summary>
    /// <param name="assembly">The assembly to scan for write side command and event handlers.</param>
    /// <param name="namespaces">Optional list of namespaces to limit the search. If not specified, the entire assembly will be scanned.</param>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration UseWriteSideAssembly(Assembly assembly, params string[] namespaces)
    {
        _writeSideSearchParams.Add(new ScanParam(assembly, namespaces));
        return this;
    }

    /// <summary>
    /// Registers a domain exception type that will be specifically recognized and handled by the system.
    /// These exceptions represent legitimate business errors rather than technical failures.
    /// </summary>
    /// <typeparam name="TException">The domain exception type to register. Must inherit from Exception.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration UseDomainException<TException>() where TException : Exception
    {
        _domainExceptionInterfaces.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Validates the configuration and throws an exception if it is not valid.
    /// A valid configuration must have at least one assembly to scan, either on the read side or on the write side.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no assembly has been specified for scanning.</exception>
    internal void ThrowIfNotValid()
    {
        if (_writeSideSearchParams.Count == 0 && _readSideSearchParams.Count == 0)
            throw new InvalidOperationException($"No assembly to scan. Use {nameof(UseWriteSideAssembly)} or {nameof(UseReadSideAssembly)} to specify on which assemblies to scan");
    }
    
    /// <summary>
    /// Registers an external command handler that produces events.
    /// This method allows integration of handlers that don't directly follow the system's convention.
    /// </summary>
    /// <param name="mapEvents">Transformation function that converts the handler's result into a collection of events.</param>
    /// <typeparam name="TCommand">The type of command to process.</typeparam>
    /// <typeparam name="THandler">The type of command handler.</typeparam>
    /// <typeparam name="TReturn">The return type of the command handler.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration MapCommandHandler<TCommand, THandler, TReturn>(Func<TReturn, IEnumerable<object>> mapEvents)
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, TReturn>(mapEvents);
        return this;
    }

    /// <summary>
    /// Registers an external command handler that produces both events and a return value.
    /// This method allows integration of complex handlers that produce multiple results.
    /// </summary>
    /// <param name="mapEvents">Transformation function that converts the handler's result into a collection of events.</param>
    /// <param name="mapValue">Transformation function that extracts a specific return value from the handler's result.</param>
    /// <typeparam name="TCommand">The type of command to process.</typeparam>
    /// <typeparam name="THandler">The type of command handler.</typeparam>
    /// <typeparam name="TReturn">The raw return type of the command handler.</typeparam>
    /// <typeparam name="TValue">The type of the extracted value to return to the client.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration MapCommandHandler<TCommand, THandler, TReturn, TValue>(
        Func<TReturn, IEnumerable<object>> mapEvents,
        Func<TReturn, TValue>? mapValue) where TValue : notnull
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, TReturn, TValue>(mapEvents, mapValue);
        return this;
    }

    /// <summary>
    /// Registers an external command handler that directly returns a collection of events.
    /// Simplified version for handlers that already follow the convention of returning events.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to process.</typeparam>
    /// <typeparam name="THandler">The type of command handler that already implements returning a collection of events.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration MapCommandHandler<TCommand, THandler>()
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, IEnumerable<object>>(x => x);
        return this;
    }

    /// <summary>
    /// Registers an external event handler for the write side.
    /// These handlers are responsible for modifying the system state in response to events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to process.</typeparam>
    /// <typeparam name="THandler">The type of event handler to register.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration MapEventHandlerOnWriteSide<TEvent, THandler>()
    {
        WriteSideEventHandlerMappings.AddMapping<TEvent, THandler>();
        return this;
    }

    /// <summary>
    /// Registers an external event handler for the read side.
    /// These handlers are responsible for updating views and projections in response to events.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to process.</typeparam>
    /// <typeparam name="THandler">The type of event handler to register.</typeparam>
    /// <returns>The current Configuration instance to enable fluent method chaining.</returns>
    public Configuration MapEventHandlerOnReadSide<TEvent, THandler>()
    {
        ReadSideEventHandlerMappings.AddMapping<TEvent, THandler>();
        return this;
    }
}