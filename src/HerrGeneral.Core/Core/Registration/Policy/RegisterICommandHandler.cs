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
            
            var interfaces = externalCommandHandler
                .GetInterfacesHavingGenericOpenType(_handlerInterface);
            
            foreach (var @interface in interfaces)
            {
                if (serviceCollection.Any(sd => sd.ServiceType == @interface))
                {
                    throw new InvalidOperationException(
                        $"Command '{@interface.GenericTypeArguments[0].Name}' already has a registered handler. A command can only have one handler.");
                }

                serviceCollection.AddTransient(
                    @interface,
                    externalCommandHandler);
            }
        }
    }
}