using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal class RegisterIVoidDomainEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = typeof(HerrGeneral.WriteSide.DDD.IVoidDomainEventHandler<>);

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalHandlers))
            return;

        foreach (var externalWriteSideEventHandler in externalHandlers)
        {
            var handlerInterfaces = externalWriteSideEventHandler.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == _handlerInterface)
                .ToList();

            if (handlerInterfaces.Count == 0)
                throw new InvalidOperationException($"Interface {_handlerInterface.Name} not found on {externalWriteSideEventHandler.GetFriendlyName()}");

            foreach (var genericArguments in 
                     handlerInterfaces
                         .Select(handlerInterface => handlerInterface.GetGenericArguments())) 
                RegisterEventHandlerServices(serviceCollection, genericArguments, externalWriteSideEventHandler);
        }
    }

    private static void RegisterEventHandlerServices(IServiceCollection serviceCollection, Type[] genericArguments, Type externalWriteSideEventHandler)
    {
        var eventType = genericArguments[0];
            
        var @interface = TypeDefinition.WriteSideEventHandlerInterface.MakeGenericType(eventType);
        var internalHandler = typeof(VoidEventHandlerInternal<,>).MakeGenericType(eventType, externalWriteSideEventHandler);

        serviceCollection.TryAddTransient(externalWriteSideEventHandler);
            
        serviceCollection.AddTransient(
            @interface,
            internalHandler);
    }
}