using System.Reflection;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all external write side event handlers present in the mappings
/// </summary>
/// <param name="eventHandlerMappings"></param>
internal class RegisterMappedReadSideEventHandlers(EventHandlerMappings eventHandlerMappings) : IRegistrationPolicy
{
    public HashSet<Type> GetOpenTypes() =>
        eventHandlerMappings
            .All()
            .Select(mapping => mapping.HandlerGenericType)
            .ToHashSet();

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlersProvider)
    {
        var scanResults =
            from mapping in eventHandlerMappings.All()
            from externalHandlerType in externalHandlersProvider[mapping.HandlerGenericType]
            from method in externalHandlerType.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)
            where method.Name == mapping.MethodInfo.Name
            let eventType = method
                .GetParameters()[0]
                .ParameterType
            select (eventType, externalHandlerType);

        foreach (var scanResult in scanResults)
        {
            var @interface = typeof(IProjectionEventHandler<>).MakeGenericType(scanResult.eventType);
            var internalHandler = typeof(ProjectionEventHandlerWithMapping<,>).MakeGenericType(scanResult.eventType, scanResult.externalHandlerType);

            serviceCollection.TryAddSingleton(scanResult.externalHandlerType);
            serviceCollection.AddTransient(@interface, internalHandler);
        }
    }
}