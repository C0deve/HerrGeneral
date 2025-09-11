using HerrGeneral.Core.DDD.RegistrationPolicies;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Extensions method for IServiceCollection
/// Registration of dynamic handlers
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Adds HerrGeneral framework services to the provided service collection.
    /// <para>Quick Start:</para>
    /// <code>
    /// services
    ///     .AddHerrGeneral(config => config
    ///         .ScanWriteSideOn(typeof(BankAccount).Assembly)
    ///         .ScanReadSideOn(typeof(AccountProjection).Assembly)
    ///         .UseDomainException&lt;DomainExceptionBase&gt;());
    /// </code>
    /// 
    /// <para>What this does:</para>
    /// <list type="bullet">
    /// <item>Set the location of your write side handlers (command and event handlers)</item>
    /// <item>Set the location of your read side event handlers</item>
    /// <item>Set the base type of your domain-specific exceptions</item>
    /// </list>
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services will be added.</param>
    /// <param name="configurationDelegate">A delegate to configure the HerrGeneral framework settings.</param>
    /// <returns>The updated service collection including the HerrGeneral services.</returns>
    public static IServiceCollection AddHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<Configuration, Configuration> configurationDelegate) =>
        new ServiceConfigurator(new RegistrationPolicyProviderForDDD())
            .ConfigureServiceCollection(serviceCollection, configurationDelegate);

    /// <summary>
    /// Custom policy manager with additional registration policies
    /// </summary>
    private class RegistrationPolicyProviderForDDD : RegistrationPolicyProvider
    {
        public override IRegistrationPolicy[] GetWriteSidePolicies(Configuration configuration) =>
        [
            ..base.GetWriteSidePolicies(configuration),
            new RegisterICreateHandler(),
            new RegisterIChangeHandler(),
            new RegisterIDomainEventHandler(),
            new RegisterIVoidDomainEventHandler(),
            new RegisterIChangeMultiHandler(),
            new RegisterDynamicCreateHandlers(),
            new RegisterDynamicChangeHandlers()
        ];
    }
}