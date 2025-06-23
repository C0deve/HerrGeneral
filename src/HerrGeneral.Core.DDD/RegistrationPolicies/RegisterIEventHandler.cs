using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.DDD.RegistrationPolicies;

internal class RegisterIEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = typeof(HerrGeneral.WriteSide.DDD.IEventHandler<>);

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        if (!externalHandlers.TryGetValue(_handlerInterface, out var handlers))
            return;

        foreach (var handlerType in handlers)
        {
            var genericArguments = handlerType.GetInterface(_handlerInterface.Name)?.GetGenericArguments()
                                   ?? throw new InvalidOperationException($"Interface {_handlerInterface.Name} not found on {handlerType.GetFriendlyName()}");
            
            var eventType = genericArguments[0];
            
            var @interface = TypeDefinition.WriteSideEventHandlerInterface.MakeGenericType(eventType);
            var internalHandler = typeof(EventHandlerInternal<,>).MakeGenericType(eventType, handlerType);

            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                internalHandler,
                ServiceLifetime.Transient));
        }
    }
}