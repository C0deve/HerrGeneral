using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal class RegisterDynamicCreateHandlers : IRegistrationPolicy
{
    private readonly Type _commandInterface = typeof(INoHandlerCreate<>);

    public HashSet<Type> GetOpenTypes() => [_commandInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_commandInterface, out var externalCommands))
            return;

        foreach (var externalCommand in externalCommands)
        {
            var @interface = externalCommand.MakeHandlerInterfaceForCreateCommand<Guid>();
            var aggregateType = externalCommand.GetAggregateTypeFromCommand();

            var dynamicHandlerType = externalCommand.MakeDynamicCreateHandlerType(aggregateType);
            var internalHandlerType = externalCommand.MakeCreateHandlerInternalType(aggregateType, dynamicHandlerType);

            serviceCollection.AddTransient(dynamicHandlerType);
            serviceCollection.AddTransient(@interface, internalHandlerType);
        }
    }
}