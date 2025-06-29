using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Extension methods for registering and configuring the HerrGeneral framework within an application.
/// Provides methods to integrate command and event handling capabilities into the dependency injection system.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Configures the HerrGeneral framework within your application's service collection.
    /// 
    /// This method performs the following operations:
    /// - Scans and registers all command handlers for processing commands
    /// - Registers write side event handlers for state-changing operations
    /// - Registers read side event handlers for updating projections and views
    /// - Adds all required internal services for the framework to operate
    /// </summary>
    /// <param name="serviceCollection">The application's service collection to which HerrGeneral services will be added</param>
    /// <param name="configurationDelegate">A delegate function that configures HerrGeneral by specifying assemblies to scan and registering custom handlers</param>
    /// <returns>The service collection with all HerrGeneral services registered</returns>
    public static IServiceCollection UseHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<Configuration, Configuration> configurationDelegate
    )
    {
        var configuration = configurationDelegate(new Configuration());
        RegisterWriteSide(serviceCollection, configuration);
        RegisterReadSide(serviceCollection, configuration);

        serviceCollection.AddSingleton<CommandLogger>();
        serviceCollection.AddSingleton<ReadSideEventDispatcher>();
        serviceCollection.AddSingleton<IAddEventToDispatch>(x => x.GetRequiredService<ReadSideEventDispatcher>());
        serviceCollection.AddSingleton<WriteSideEventDispatcher>();
        serviceCollection.AddSingleton<Mediator>();
        serviceCollection.AddSingleton<DomainExceptionMapper>(_ => new DomainExceptionMapper(configuration.DomainExceptionTypes.ToArray()));
        serviceCollection.AddSingleton<CommandHandlerMappings>(_ => configuration.CommandHandlerMappings);
        serviceCollection.AddSingleton<IWriteSideEventHandlerMappings>(_ => new EventHandlerMappings(configuration.WriteSideEventHandlerMappings));
        serviceCollection.AddSingleton<IReadSideEventHandlerMappings>(_ => new EventHandlerMappings(configuration.ReadSideEventHandlerMappings));

        return serviceCollection;
    }

    private static void RegisterWriteSide(IServiceCollection serviceCollection, Configuration configuration)
    {
        IRegistrationPolicy[] policies = [
            new RegisterMappedCommandHandlers(configuration.CommandHandlerMappings),
            new RegisterICommandHandler(),
            new RegisterMappedWriteSideEventHandlers(configuration.WriteSideEventHandlerMappings),
            new RegisterWriteSideEventHandler()
        ];
        
        Register(serviceCollection, policies, configuration.WriteSideSearchParams);
    }

    private static void RegisterReadSide(IServiceCollection serviceCollection, Configuration configuration)
    {
        IRegistrationPolicy[] policies = [
            new RegisterReadSideEventHandler(),
            new RegisterMappedReadSideEventHandlers(configuration.ReadSideEventHandlerMappings),
        ];

        Register(serviceCollection, policies, configuration.ReadSideSearchParams);
    }
    
    internal static void Register(IServiceCollection serviceCollection, IRegistrationPolicy[] policies, IEnumerable<ScanParam> scanParams)
    {
        var openTypesToScan = policies
            .SelectMany(policy => policy.GetOpenTypes())
            .ToHashSet();
        var externalHandlers = Scanner.Scan(scanParams, openTypesToScan);

        foreach (var policy in policies) 
            policy.Register(serviceCollection, externalHandlers);
    }

    internal static void RegisterOpenType(this IServiceCollection serviceCollection, Type tHandler, Type openTypeInterface, ServiceLifetime serviceLifetime)
    {
        var interfacesToRegister = tHandler.GetInterfacesHavingGenericOpenType(openTypeInterface);
        if (serviceLifetime == ServiceLifetime.Singleton)
        {
            serviceCollection.AddSingleton(tHandler);
            serviceCollection.RegisterOpenTypeInternal(tHandler, ServiceLifetime.Transient, interfacesToRegister);
        }
        else
            serviceCollection.RegisterOpenTypeInternal(tHandler, serviceLifetime, interfacesToRegister);
    }

    /// <summary>
    /// Registers a handler type against all its implemented interfaces in the service collection.
    /// This method creates service descriptors that map each interface to the same handler implementation instance.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register the handler with</param>
    /// <param name="tHandler">The handler type to register</param>
    /// <param name="serviceLifetime">The lifetime scope for the registered services</param>
    /// <param name="interfaces">The collection of interfaces implemented by the handler that should be registered</param>
    private static void RegisterOpenTypeInternal(this IServiceCollection serviceCollection, Type tHandler, ServiceLifetime serviceLifetime, IEnumerable<Type> interfaces)
    {
        foreach (var @interface in interfaces)
            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                provider => provider.GetRequiredService(tHandler),
                serviceLifetime));
    }
}