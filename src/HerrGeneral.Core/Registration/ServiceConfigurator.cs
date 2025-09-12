using HerrGeneral.Core.Configuration;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Configures services for the application using specified registration policies and settings.
/// </summary>
internal class ServiceConfigurator(RegistrationPolicyProvider policyProvider)
{
    public IServiceCollection ConfigureServiceCollection(IServiceCollection serviceCollection, Configuration.Configuration configuration)
    {
        RegisterWriteSide(serviceCollection, configuration, policyProvider);
        RegisterReadSide(serviceCollection, configuration, policyProvider);

        if (configuration.IsTracingEnabled)
            serviceCollection.AddScoped<CommandExecutionTracer>();
        serviceCollection.AddScoped<ReadSideEventDispatcher>();
        serviceCollection.AddScoped<WriteSideEventDispatcher>();
        serviceCollection.AddSingleton<Mediator>();
        serviceCollection.AddSingleton<DomainExceptionMapper>(_ => new DomainExceptionMapper(configuration.DomainExceptionTypes.ToArray()));
        serviceCollection.AddSingleton<CommandHandlerMappings>(_ => configuration.CommandHandlerMappings);
        serviceCollection.AddSingleton<IWriteSideEventHandlerMappings>(_ => new EventHandlerMappingsProvider(configuration.WriteSideEventHandlerMappingsConfiguration));
        serviceCollection.AddSingleton<IReadSideEventHandlerMappings>(_ => new EventHandlerMappingsProvider(configuration.ReadSideEventHandlerMappingsConfiguration));

        return serviceCollection;
    }

    private static void RegisterWriteSide(
        IServiceCollection serviceCollection, 
        Configuration.Configuration configuration, 
        RegistrationPolicyProvider policyProvider)
    {
        var policies = policyProvider.GetWriteSidePolicies(configuration);
        ServiceExtension.Register(serviceCollection, policies, configuration.WriteSideSearchParams);
    }

    private static void RegisterReadSide(
        IServiceCollection serviceCollection, 
        Configuration.Configuration configuration, 
        RegistrationPolicyProvider policyProvider)
    {
        var policies = policyProvider.GetReadSidePolicies(configuration);
        Register(serviceCollection, policies, configuration.ReadSideSearchParams);
    }
    
    private static void Register(IServiceCollection serviceCollection, IRegistrationPolicy[] policies, IEnumerable<ScanParam> scanParams)
    {
        var openTypesToScan = policies
            .SelectMany(policy => policy.GetOpenTypes())
            .ToHashSet();
        var externalHandlers = Scanner.Scan(scanParams, openTypesToScan);

        if (externalHandlers.Count == 0) return;
        
        foreach (var policy in policies)
            policy.Register(serviceCollection, externalHandlers);
    }
}