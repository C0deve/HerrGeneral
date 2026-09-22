using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all handlers on read side that implement <see cref="IHandlePostProjection{TEvent}"/>.
/// </summary>
internal class RegisterIHandlePostProjection : IRegistrationPolicy
{
    private readonly Type _handlerInterface = TypeDefinition.PostProjectionHandlerInterface;

    public HashSet<Type> GetOpenTypes() => [_handlerInterface];

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        if (!externalHandlersProvider.TryGetValue(_handlerInterface, out var externalHandlers))
            return;

        foreach (var externalHandler in externalHandlers)
        {
            serviceCollection.TryAddTransient(externalHandler);

            foreach (var @interface in externalHandler
                         .GetInterfacesHavingGenericOpenType(_handlerInterface))
            {
                serviceCollection.AddTransient(
                    @interface,
                    p => p.GetRequiredService(externalHandler));
            }
        }
    }
}
