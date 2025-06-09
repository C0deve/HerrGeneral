using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.DDD.RegistrationPolicies;

internal class RegisterICreateHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = typeof(ICreateHandler<,>);

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        if (!externalHandlers.TryGetValue(_handlerInterface, out var handlers)) 
            return;
        
        foreach (var commandHandler in handlers)
        {
            var genericArguments = commandHandler.GetInterface(_handlerInterface.Name)?.GetGenericArguments() 
                                   ?? throw new InvalidOperationException($"Interface {_handlerInterface.Name} not found on {commandHandler.GetFriendlyName()}");
            var @interface = TypeDefinition.CommandHandlerInterface.MakeGenericType(genericArguments[1], typeof(Guid));
            var internalHandler = typeof(CreateHandlerInternal<,,>).MakeGenericType(genericArguments[0], genericArguments[1], commandHandler);

            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                internalHandler,
                ServiceLifetime.Transient));
        }
    }
}