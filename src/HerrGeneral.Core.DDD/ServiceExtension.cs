using System.Reflection;
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
    /// Register all dynamic handler
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterDynamicHandlers(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var types = Select<Guid>(assembly);

        foreach (var (@interface, type) in types)
            serviceCollection.TryAddTransient(
                @interface,
                type);

        return serviceCollection;
    }

    private static IEnumerable<(Type Interface, Type HandlerType)> Select<TResult>(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsCreateCommand())
            {
                yield return (
                    Interface: type.MakeHandlerInterfaceForCreateCommand<TResult>(),
                    HandlerType: type.MakeDynamicHandlerForCreateCommand());
                continue; 
            }

            if (!type.IsChangeCommand()) 
                continue;
            
            yield return (
                Interface: type.MakeHandlerInterfaceForChangeCommand(),
                HandlerType: type.MakeDynamicHandlerForChangeCommand());
        }
    }
    
    private static bool IsCreateCommand(this Type type) =>
        type.BaseType?.IsGenericType == true &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Create<>);

    private static Type MakeHandlerInterfaceForCreateCommand<TResult>(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

    private static Type MakeDynamicHandlerForCreateCommand(this Type commandType) =>
        typeof(CreateHandlerDynamic<,>).MakeGenericType(commandType.GetAggregateTypeFromCommand(), commandType);
    
    private static bool IsChangeCommand(this Type type) =>
        type.BaseType?.IsGenericType == true &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Change<>);

    private static Type MakeHandlerInterfaceForChangeCommand(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Unit));

    private static Type MakeDynamicHandlerForChangeCommand(this Type commandType) =>
        typeof(ChangeHandlerDynamic<,>).MakeGenericType(commandType.GetAggregateTypeFromCommand(), commandType);

    private static Type GetAggregateTypeFromCommand(this Type commandType) =>
        commandType.BaseType!.GetGenericArguments()[0];
}