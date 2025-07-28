using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handler on read side that inherits <see cref="HerrGeneral.WriteSide.IEventHandler{TEvent}"/>
/// </summary>
internal class RegisterWriteSideEventHandler : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.WriteSideEventHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalWriteSideEventHandlers))
            return;

        foreach (var externalEventHandler in externalWriteSideEventHandlers)
        {
            serviceCollection.TryAddTransient(externalEventHandler);

            foreach (var @interface in externalEventHandler
                         .GetInterfacesHavingGenericOpenType(_handlerInterface))
            {
                serviceCollection.AddTransient(
                    @interface,
                    externalEventHandler);
            }
        }
    }
}