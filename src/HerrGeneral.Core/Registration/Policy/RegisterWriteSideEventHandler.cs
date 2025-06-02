using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handler on read side that inherits <see cref="HerrGeneral.WriteSide.IEventHandler{TEvent}"/>
/// </summary>
internal class RegisterWriteSideEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.WriteSideEventHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];
    
    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        if (!externalHandlers.TryGetValue(_handlerInterface, out var handlers)) 
            return;
        
        foreach (var eventHandler in handlers)
            serviceCollection.RegisterOpenType(
                eventHandler,
                _handlerInterface,
                ServiceLifetime.Transient);
    }
}