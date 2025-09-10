using System.Reflection;
using HerrGeneral.Core.DDD.RegistrationPolicies;
using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Extensions method for IServiceCollection
/// Registration of dynamic handlers
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Checks if a service interface is already registered in the service collection
    /// </summary>
    /// <typeparam name="TService">The service type to check</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>true if the service is already registered, otherwise false</returns>
    public static bool IsServiceRegistered<TService>(this IServiceCollection services)
    {
        return services.Any(d => d.ServiceType == typeof(TService));
    }

    /// <summary>
    /// Checks if a service interface is already registered in the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="serviceType">The service type to check</param>
    /// <returns>true if the service is already registered, otherwise false</returns>
    public static bool IsServiceRegistered(this IServiceCollection services, Type serviceType)
    {
        return services.Any(d => d.ServiceType == serviceType);
    }
    
    /// <summary>
    /// Registers commands without declared handler
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDynamicHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        RegistrationPolicies.RegisterDynamicHandlers.Register(serviceCollection, assembly);

        return serviceCollection;
    }
    
    /// <summary>
    /// Registers all handlers inheriting <see cref="ICreateHandler{TAggregate,TCommand}"/> and <see cref="IChangeHandler{TAggregate,TCommand}"/> 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDDDHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        Registration.ServiceExtension.Register(
            serviceCollection, 
            [
                new RegisterICreateHandler(), 
                new RegisterIChangeHandler(), 
                new RegisterIEventHandler(),
                new RegisterIChangeMultiHandler()
            ], 
            [new ScanParam(assembly)]);
        
        return serviceCollection;
    }
}