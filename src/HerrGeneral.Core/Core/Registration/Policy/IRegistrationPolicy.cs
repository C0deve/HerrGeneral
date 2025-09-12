using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Manage registration of all handlers that implements open type provided by GetOpenTypes()
/// </summary>
internal interface IRegistrationPolicy
{
    /// <summary>
    /// Returns base open types affected by this policy
    /// </summary>
    /// <returns></returns>
    HashSet<Type> GetOpenTypes();

    /// <summary>
    /// Register all concrete class that inherits one of the provided open type
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="externalHandlersProvider">Mapping between open type and their concrete class</param>
    void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider);
}