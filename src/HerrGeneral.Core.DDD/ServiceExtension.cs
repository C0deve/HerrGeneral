using System.Reflection;
using HerrGeneral.Core.DDD.RegistrationPolicies;
using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Extensions method for IServiceCollection
/// Registration of dynamic handlers
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Registers commands without declared handler
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDynamicHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var types = MakeHandlerTypesForCreateAndChangeCommand(assembly);

        foreach (var (@interface, type) in types)
            serviceCollection.TryAddTransient(
                @interface,
                type);

        return serviceCollection;
    }
    
    /// <summary>
    /// Registers all handlers inheriting <see cref="ICreateHandler{TAggregate,TCommand}"/> and <see cref="IChangeHandler{TAggregate,TCommand}"/> 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDDDHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        Registration.ServiceExtension.Register(
            serviceCollection, 
            [new RegisterICreateHandler(), new RegisterIChangeHandler()], 
            [new ScanParam(assembly)]);
        
        return serviceCollection;
    }

    private static IEnumerable<(Type Interface, Type HandlerType)> MakeHandlerTypesForCreateAndChangeCommand(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsCreateCommand())
            {
                yield return (
                    Interface: type.MakeHandlerInterfaceForCreateCommand<Guid>(),
                    HandlerType: type.MakeCreateHandlerType());
                continue; 
            }

            if (!type.IsChangeCommand()) 
                continue;
            
            yield return (
                Interface: type.MakeHandlerInterfaceForChangeCommand(),
                HandlerType: type.MakeChangeHandlerType());
        }
    }
    
    private static bool IsCreateCommand(this Type type) =>
        type.BaseType?.IsGenericType == true &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Create<>);

    private static Type MakeHandlerInterfaceForCreateCommand<TResult>(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

    private static Type MakeCreateHandlerType(this Type commandType)
    {
        var aggregateType = commandType.GetAggregateTypeFromCommand();
        var dynamicHandler = typeof(CreateHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);
        return typeof(CreateHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);
    }

    private static bool IsChangeCommand(this Type type) =>
        type.BaseType?.IsGenericType == true &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Change<>);

    private static Type MakeHandlerInterfaceForChangeCommand(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Unit));

    private static Type MakeChangeHandlerType(this Type commandType)
    {
        var aggregateType = commandType.GetAggregateTypeFromCommand();
        var dynamicHandler = typeof(ChangeHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);
        return typeof(ChangeHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);
    }

    private static Type GetAggregateTypeFromCommand(this Type commandType) =>
        commandType.BaseType!.GetGenericArguments()[0];
}