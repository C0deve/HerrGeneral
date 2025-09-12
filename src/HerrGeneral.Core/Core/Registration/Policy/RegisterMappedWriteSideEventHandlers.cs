using HerrGeneral.Core.Configuration;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all external write side event handlers present in the mappings
/// </summary>
/// <param name="eventHandlerMappingsConfiguration"></param>
internal class RegisterMappedWriteSideEventHandlers(EventHandlerMappingsConfiguration eventHandlerMappingsConfiguration) : IRegistrationPolicy
{
    public HashSet<Type> GetOpenTypes() =>
        eventHandlerMappingsConfiguration
            .All()
            .Select(mapping => mapping.HandlerGenericType)
            .ToHashSet();

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        var scanResults =
            from mapping in eventHandlerMappingsConfiguration.All()
            from externalHandlerType in externalHandlersProvider[mapping.HandlerGenericType]
            let eventType = externalHandlerType
                .GetMethod(mapping.MethodInfo.Name)!
                .GetParameters()[0]
                .ParameterType
            select (eventType, externalHandlerType);

        foreach (var scanResult in scanResults)
        {
            var @interface = typeof(IEventHandler<>).MakeGenericType(scanResult.eventType);
            var internalHandler = typeof(EventHandlerWithMapping<,>).MakeGenericType(scanResult.eventType, scanResult.externalHandlerType);
            
            serviceCollection.TryAddTransient(scanResult.externalHandlerType);
            
            serviceCollection.AddTransient(
                @interface,
                internalHandler);
        }
    }
}