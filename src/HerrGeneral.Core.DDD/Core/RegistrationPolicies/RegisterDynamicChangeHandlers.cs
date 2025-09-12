using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal class RegisterDynamicChangeHandlers : IRegistrationPolicy
{
    private readonly Type _commandInterface = typeof(INoHandlerChange<>);

    public HashSet<Type> GetOpenTypes() => [_commandInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_commandInterface, out var externalCommands))
            return;

        foreach (var externalCommand in externalCommands)
        {
            var @interface = externalCommand.MakeHandlerInterfaceForChangeCommand();
            var aggregateType = externalCommand.GetAggregateTypeFromCommand();

            var dynamicHandlerType = externalCommand.MakeDynamicChangeHandlerType(aggregateType);
            var internalHandlerType = externalCommand.MakeChangeHandlerType(aggregateType, dynamicHandlerType);

            serviceCollection.AddTransient(dynamicHandlerType);
            serviceCollection.AddTransient(@interface, internalHandlerType);
        }
    }
}