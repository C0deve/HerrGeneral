using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Core.Registration.Policy;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.DDD.Core.RegistrationPolicies;

internal class RegisterICreateHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = typeof(ICreateHandler<,>);

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalHandlers)) 
            return;
        
        foreach (var externalCommandHandler in externalHandlers)
        {
            var genericArguments = externalCommandHandler.GetInterface(_handlerInterface.Name)?.GetGenericArguments() 
                                   ?? throw new InvalidOperationException($"Interface {_handlerInterface.Name} not found on {externalCommandHandler.GetFriendlyName()}");
            var @interface = TypeDefinition.CommandHandlerInterface.MakeGenericType(genericArguments[1], typeof(Guid));
            var internalHandler = typeof(CreateHandlerInternal<,,>).MakeGenericType(genericArguments[0], genericArguments[1], externalCommandHandler);

            serviceCollection.TryAddTransient(externalCommandHandler);
            
            serviceCollection.AddTransient(
                @interface,
                internalHandler);
        }
    }
}