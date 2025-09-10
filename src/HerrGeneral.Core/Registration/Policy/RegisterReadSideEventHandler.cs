using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handler on read side that inherits <see cref="HerrGeneral.ReadSide.IEventHandler{TEvent}"/>
/// </summary>
internal class RegisterReadSideEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.ReadSideEventHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalReadSideEventHandlers))
            return;

        foreach (var externalEventHandler in externalReadSideEventHandlers)
        {
            foreach (var @interface in externalEventHandler
                         .GetInterfacesHavingGenericOpenType(_handlerInterface))
            {
                serviceCollection.AddTransient(
                    @interface,
                    p => p.GetRequiredService(externalEventHandler));
            }
        }
    }
}