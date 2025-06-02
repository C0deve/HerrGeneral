using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handler on read side that inherits <see cref="HerrGeneral.ReadSide.IEventHandler{TEvent}"/>
/// </summary>
internal class RegisterReadSideEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.ReadSideEventHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        if (!externalHandlers.TryGetValue(_handlerInterface, out var handlers)) 
            return;
        
        foreach (var eventHandler in handlers)
            serviceCollection.RegisterOpenType(
                eventHandler,
                _handlerInterface,
                ServiceLifetime.Singleton);
    }
}