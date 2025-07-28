using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handlers that inherits <see cref="ICommandHandler{TCommand,TResult}"/>
/// </summary>
internal class RegisterICommandHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.CommandHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalCommandHandlers)) 
            return;
        
        foreach (var externalCommandHandler in externalCommandHandlers)
        {
            serviceCollection.TryAddTransient(externalCommandHandler);
            
            var @interface = externalCommandHandler
                .GetInterfacesHavingGenericOpenType(_handlerInterface)
                .Single();
            
            serviceCollection.AddTransient(
                @interface,
                externalCommandHandler);
        }
    }
}