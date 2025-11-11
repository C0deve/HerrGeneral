using HerrGeneral.Core.Registration;
using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Core.Configuration;

/// <summary>
/// Internal configuration for HerrGeneral.
/// </summary>
/// <param name="WriteSideSearchParams">Set of search parameters for write side assemblies. Contains the assemblies and namespaces to scan for discovering event and command handlers.</param>
/// <param name="ReadSideSearchParams">Set of search parameters for read side assemblies. Contains the assemblies and namespaces to scan for discovering read side event handlers.</param>
/// <param name="DomainExceptionTypes">Set of registered domain exception types. These types are used to identify and handle exceptions specific to the business domain.</param>
/// <param name="CommandHandlerMappings">Collection of mappings for external command handlers. Allows integration of command handlers from external sources into the system.</param>
/// <param name="WriteSideEventHandlerMappingsConfiguration">Collection of mappings for external write side event handlers. Allows registration of event handlers that modify system state.</param>
/// <param name="ReadSideEventHandlerMappingsConfiguration">Collection of mappings for external read side event handlers. Facilitates updating views and projections by registering appropriate event handlers.</param>
/// <param name="IsTracingEnabled">Indicates whether execution tracing for command handlers is enabled. Enables detailed logging of command processing for debugging and performance monitoring.</param>
/// <param name="MaxConcurrentCommands">Defines the maximum number of concurrently running commands allowed by the mediator.</param>
internal record Configuration(
    IReadOnlyCollection<ScanParam> WriteSideSearchParams,
    IReadOnlyCollection<ScanParam> ReadSideSearchParams,
    IReadOnlySet<Type> DomainExceptionTypes,
    CommandHandlerMappings CommandHandlerMappings,
    EventHandlerMappingsConfiguration WriteSideEventHandlerMappingsConfiguration,
    EventHandlerMappingsConfiguration ReadSideEventHandlerMappingsConfiguration,
    bool IsTracingEnabled,
    short MaxConcurrentCommands
);