using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Extension methods for registration
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Configure HerrGeneral :
    /// Scan and register all command handlers, write side event handlers and read side event handlers 
    /// Add required services 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configurationDelegate"></param>
    /// <returns></returns>
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
        serviceCollection.AddSingleton<HandlerMappings>(_ => configuration.HandlerMappings);

        return serviceCollection;
    }

    private static void RegisterWriteSide(IServiceCollection serviceCollection, Configuration configuration)
    {
        IRegistrationPolicy[] policies = [
            new RegisterMappedCommandHandlers(configuration.HandlerMappings),
            new RegisterICommandHandler(),
            new RegisterWriteSideEventHandler()
        ];
        
        Register(serviceCollection, policies, configuration.WriteSideSearchParams);
    }

    private static void RegisterReadSide(IServiceCollection serviceCollection, Configuration configuration)
    {
        IRegistrationPolicy[] policies = [
            new RegisterReadSideEventHandler()
        ];

        Register(serviceCollection, policies, configuration.ReadSideSearchParams);
    }
    
    private static void Register(IServiceCollection serviceCollection, IRegistrationPolicy[] policies, IEnumerable<ScanParam> scanParams)
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
    /// Register <see cref="tHandler"/> for all his interfaces/>  
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="tHandler"></param>
    /// <param name="serviceLifetime"></param>
    /// <param name="interfaces"></param>
    private static void RegisterOpenTypeInternal(this IServiceCollection serviceCollection, Type tHandler, ServiceLifetime serviceLifetime, IEnumerable<Type> interfaces)
    {
        foreach (var @interface in interfaces)
            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                provider => provider.GetRequiredService(tHandler),
                serviceLifetime));
    }
}