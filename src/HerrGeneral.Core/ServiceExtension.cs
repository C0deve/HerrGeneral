using HerrGeneral.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Registration;

/// <summary>
/// Extension methods for registering and configuring the HerrGeneral framework within an application.
/// Provides methods to integrate command and event handling capabilities into the dependency injection system.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Adds HerrGeneral framework services to the provided service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services will be added.</param>
    /// <param name="configurationDelegate">A delegate to configure the HerrGeneral framework settings.</param>
    /// <returns>The updated service collection including the HerrGeneral services.</returns>
    public static IServiceCollection AddHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<ConfigurationBuilder, ConfigurationBuilder> configurationDelegate) =>
        new ServiceConfigurator(new RegistrationPolicyProvider())
            .ConfigureServiceCollection(
                serviceCollection,
                configurationDelegate(new ConfigurationBuilder()).Build()
            );
}