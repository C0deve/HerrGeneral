using HerrGeneral.Core.Registration.Policy;

namespace HerrGeneral;

/// <summary>
/// Builder interface for configuring the HerrGeneral framework and extending it with modular plugins and behaviors.
/// </summary>
public interface IHerrGeneralBuilder
{
    /// <summary>
    /// Gets the service collection for dependency injection registrations.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the property bag for sharing state and configuration between extensions.
    /// </summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>
    /// Registers a custom registration policy for discovering and registering handler types.
    /// </summary>
    /// <param name="policy">The registration policy to add.</param>
    /// <returns>The builder instance for chaining.</returns>
    IHerrGeneralBuilder RegisterPolicy(IRegistrationPolicy policy);
}
