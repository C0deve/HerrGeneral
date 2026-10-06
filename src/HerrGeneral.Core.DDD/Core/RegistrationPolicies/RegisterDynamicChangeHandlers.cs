using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal sealed class RegisterDynamicChangeHandlers : IRegistrationPolicy
{
    private readonly Type _commandInterface = typeof(INoHandlerChange<>);

    public HashSet<Type> GetOpenTypes() => [_commandInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_commandInterface, out var externalCommands))
            return;

        foreach (var externalCommand in externalCommands)
        {
            var (aggregateType, keyType) = externalCommand.GetAggregateAndKeyTypeFromChangeCommand();
            var @interface = TypeDefinition.CommandHandlerInterface.MakeGenericType(externalCommand, typeof(Unit));

            var dynamicHandlerType = typeof(ChangeHandlerByReflection<,,>).MakeGenericType(aggregateType, keyType, externalCommand);
            var internalHandlerType = typeof(ChangeHandlerInternal<,,,>).MakeGenericType(aggregateType, keyType, externalCommand, dynamicHandlerType);

            serviceCollection.TryAddAggregateDependencies(aggregateType, keyType);
            serviceCollection.AddTransient(dynamicHandlerType);
            serviceCollection.AddTransient(@interface, internalHandlerType);
        }
    }
}