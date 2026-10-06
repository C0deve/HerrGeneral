using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal sealed class RegisterIChangeMultiHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface3 = typeof(IChangeMultiHandler<,,>);
    private readonly Type _handlerInterface2 = typeof(IChangeMultiHandler<,>);

    public HashSet<Type> GetOpenTypes() => [_handlerInterface3, _handlerInterface2];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        var allHandlers = new HashSet<Type>();
        if (externalHandlersProvider.TryGetValue(_handlerInterface3, out var handlers3))
            allHandlers.UnionWith(handlers3);
        if (externalHandlersProvider.TryGetValue(_handlerInterface2, out var handlers2))
            allHandlers.UnionWith(handlers2);

        if (allHandlers.Count == 0)
            return;

        foreach (var externalCommandHandler in allHandlers)
        {
            var handlerInterface = externalCommandHandler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == _handlerInterface3)
                ?? externalCommandHandler.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == _handlerInterface2);

            if (handlerInterface == null)
                throw new InvalidOperationException($"Interface IChangeMultiHandler not found on {externalCommandHandler.GetFriendlyName()}");

            var genericArguments = handlerInterface.GetGenericArguments();
            Type aggregateType = genericArguments[0];
            Type commandType = genericArguments[1];
            Type keyType = genericArguments.Length >= 3 ? genericArguments[2] : (aggregateType.GetKeyTypeFromAggregate() ?? typeof(Guid));

            var @interface = TypeDefinition.CommandHandlerInterface.MakeGenericType(commandType, typeof(Unit));
            var internalHandler = typeof(ChangeMultiHandlerInternal<,,,>).MakeGenericType(aggregateType, keyType, commandType, externalCommandHandler);
            
            serviceCollection.TryAddAggregateDependencies(aggregateType, keyType);
            serviceCollection.TryAddTransient(externalCommandHandler);
            serviceCollection.AddTransient(@interface, internalHandler);
        }
    }
}