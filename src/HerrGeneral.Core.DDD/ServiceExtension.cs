using HerrGeneral.DDD.Core.RegistrationPolicies;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.DDD;

/// <summary>
/// Extensions method for configuring HerrGeneral with DDD support.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Enables Domain-Driven Design (DDD) support, registering aggregate create/change handlers and domain event handlers.
    /// </summary>
    /// <param name="builder">The HerrGeneral builder.</param>
    /// <returns>The builder instance for chaining.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IHerrGeneralBuilder UseDDD(this IHerrGeneralBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .RegisterPolicy(new RegisterICreateHandler())
            .RegisterPolicy(new RegisterIChangeHandler())
            .RegisterPolicy(new RegisterIDomainEventHandler())
            .RegisterPolicy(new RegisterIVoidDomainEventHandler())
            .RegisterPolicy(new RegisterIChangeMultiHandler())
            .RegisterPolicy(new RegisterDynamicCreateHandlers())
            .RegisterPolicy(new RegisterDynamicChangeHandlers())
            .RegisterPolicy(new RegisterICrossAggregateChangeHandler())
            .RegisterPolicy(new RegisterIHandleCrossAggregate());
    }

    /// <summary>
    /// Enables Domain-Driven Design (DDD) support, registering aggregate create/change handlers and domain event handlers.
    /// </summary>
    /// <param name="builder">The ConfigurationBuilder.</param>
    /// <returns>The ConfigurationBuilder instance for chaining.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static ConfigurationBuilder UseDDD(this ConfigurationBuilder builder)
    {
        ((IHerrGeneralBuilder)builder).UseDDD();
        return builder;
    }

    /// <summary>
    /// Adds HerrGeneral framework services with DDD support to the provided service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which the services will be added.</param>
    /// <param name="configurationDelegate">A delegate to configure the HerrGeneral framework settings.</param>
    /// <returns>The updated service collection including the HerrGeneral services.</returns>
    public static IServiceCollection AddHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<ConfigurationBuilder, ConfigurationBuilder> configurationDelegate) =>
        Registration.ServiceExtension.AddHerrGeneral(serviceCollection, cfg =>
        {
            cfg.UseDDD();
            return configurationDelegate(cfg);
        });
}