using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handlers that inherits <see cref="ICommandHandler{TCommand,TResult}"/>
/// </summary>
internal class RegisterICommandHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.CommandHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        if (!externalHandlers.TryGetValue(_handlerInterface, out var handlers)) 
            return;
        
        foreach (var commandHandler in handlers)
            serviceCollection.RegisterOpenType(
                commandHandler,
                _handlerInterface,
                ServiceLifetime.Transient);
    }
}