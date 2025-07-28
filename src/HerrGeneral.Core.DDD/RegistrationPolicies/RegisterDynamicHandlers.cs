using System.Reflection;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.DDD.RegistrationPolicies;

internal static class RegisterDynamicHandlers
{
    public static void Register(IServiceCollection serviceCollection, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAssignableToGenericType(typeof(Create<>)))
                serviceCollection.RegisterDynamicCreateHandler(type);

            else if (type.IsAssignableToGenericType(typeof(Change<>)))
                serviceCollection.RegisterDynamicChangeHandler(type);
        }
    }

    private static void RegisterDynamicChangeHandler(this IServiceCollection serviceCollection, Type type)
    {
        var @interface = type.MakeHandlerInterfaceForChangeCommand();

        if (serviceCollection.IsServiceRegistered(@interface))
            return;

        var aggregateType = type.GetAggregateTypeFromCommand();

        var dynamicHandlerType = type.MakeDynamicChangeHandlerType(aggregateType);
        var internalHandlerType = type.MakeChangeHandlerType(aggregateType, dynamicHandlerType);

        serviceCollection.AddTransient(dynamicHandlerType);
        serviceCollection.AddTransient(@interface, internalHandlerType);
    }

    private static void RegisterDynamicCreateHandler(this IServiceCollection serviceCollection, Type type)
    {
        var @interface = type.MakeHandlerInterfaceForCreateCommand<Guid>();

        if (serviceCollection.IsServiceRegistered(@interface))
            return;

        var aggregateType = type.GetAggregateTypeFromCommand();

        var dynamicHandlerType = type.MakeDynamicCreateHandlerType(aggregateType);
        var internalHandlerType = type.MakeCreateHandlerInternalType(aggregateType, dynamicHandlerType);

        serviceCollection.AddTransient(dynamicHandlerType);
        serviceCollection.AddTransient(@interface, internalHandlerType);
    }
}