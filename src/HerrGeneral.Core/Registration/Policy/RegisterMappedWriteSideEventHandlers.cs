using HerrGeneral.Core.WriteSide;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all external write side event handlers present in the mappings
/// </summary>
/// <param name="eventHandlerMappings"></param>
internal class RegisterMappedWriteSideEventHandlers(EventHandlerMappings eventHandlerMappings) : IRegistrationPolicy
{
    public HashSet<Type> GetOpenTypes() =>
        eventHandlerMappings
            .All()
            .Select(mapping => mapping.HandlerGenericType)
            .ToHashSet();

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        var scanResults =
            from mapping in eventHandlerMappings.All()
            from externalHandlerType in externalHandlers[mapping.HandlerGenericType]
            let eventType = externalHandlerType
                .GetMethod(mapping.MethodInfo.Name)!
                .GetParameters()[0]
                .ParameterType
            select (eventType, externalHandlerType);

        foreach (var scanResult in scanResults)
        {
            var @interface = typeof(IEventHandler<>).MakeGenericType(scanResult.eventType);
            var internalHandler = typeof(EventHandlerWithMapping<,>).MakeGenericType(scanResult.eventType, scanResult.externalHandlerType);

            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                internalHandler,
                ServiceLifetime.Transient));
        }
    }
}