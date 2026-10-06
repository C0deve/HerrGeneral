using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal sealed class RegisterICrossAggregateChangeHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface3 = typeof(ICrossAggregateChangeHandler<,,>);
    private readonly Type _handlerInterface2 = typeof(ICrossAggregateChangeHandler<,>);

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

        foreach (var externalWriteSideEventHandler in allHandlers)
        {
            var handlerInterfaces = externalWriteSideEventHandler.GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == _handlerInterface3 || i.GetGenericTypeDefinition() == _handlerInterface2))
                .ToList();

            if (handlerInterfaces.Count == 0)
                throw new InvalidOperationException($"Interface ICrossAggregateChangeHandler not found on {externalWriteSideEventHandler.GetFriendlyName()}");

            foreach (var genericArguments in 
                     handlerInterfaces
                         .Select(handlerInterface => handlerInterface.GetGenericArguments())) 
                RegisterEventHandlerServices(serviceCollection, genericArguments, externalWriteSideEventHandler);
        }
    }

    private static void RegisterEventHandlerServices(IServiceCollection serviceCollection, Type[] genericArguments, Type externalWriteSideEventHandler)
    {
        var eventType = genericArguments[0];
        var aggregateType = genericArguments[1];
        var @interface = TypeDefinition.WriteSideEventHandlerInterface.MakeGenericType(eventType);

        Type internalHandler;
        if (genericArguments.Length >= 3)
        {
            var keyType = genericArguments[2];
            internalHandler = typeof(CrossAggregateChangeHandlerInternal<,,,>).MakeGenericType(eventType, externalWriteSideEventHandler, aggregateType, keyType);
        }
        else
        {
            internalHandler = typeof(CrossAggregateChangeHandlerInternal<,,>).MakeGenericType(eventType, externalWriteSideEventHandler, aggregateType);
        }

        serviceCollection.TryAddTransient(externalWriteSideEventHandler);
            
        serviceCollection.AddTransient(
            @interface,
            internalHandler);
    }
}