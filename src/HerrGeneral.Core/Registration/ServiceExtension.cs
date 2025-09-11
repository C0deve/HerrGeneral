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
    /// Adds HerrGeneral framework services to the provided service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services will be added.</param>
    /// <param name="configurationDelegate">A delegate to configure the HerrGeneral framework settings.</param>
    /// <returns>The updated service collection including the HerrGeneral services.</returns>
    public static IServiceCollection AddHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<Configuration, Configuration> configurationDelegate) =>
        new ServiceConfigurator(new RegistrationPolicyProvider())
            .ConfigureServiceCollection(serviceCollection, configurationDelegate);

    internal static void Register(IServiceCollection serviceCollection, IRegistrationPolicy[] policies, IEnumerable<ScanParam> scanParams)
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