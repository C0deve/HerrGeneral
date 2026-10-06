namespace HerrGeneral.DDD.Core;

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

internal static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, (Type AggregateType, Type KeyType)> CreateCommandCache = new();
    private static readonly ConcurrentDictionary<Type, (Type AggregateType, Type KeyType)> ChangeCommandCache = new();
    private static readonly ConcurrentDictionary<Type, Type?> AggregateKeyTypeCache = new();
    private static readonly ConcurrentDictionary<Type, Type> CommandAggregateTypeCache = new();

    public static void TryAddAggregateDependencies(this IServiceCollection serviceCollection, Type aggregateType, Type keyType)
    {
        if (keyType == typeof(Guid))
        {
            var repo2 = typeof(IAggregateRepository<,>).MakeGenericType(aggregateType, typeof(Guid));
            var repo1 = typeof(IAggregateRepository<>).MakeGenericType(aggregateType);
            serviceCollection.TryAddTransient(repo2, sp => sp.GetRequiredService(repo1));
            serviceCollection.TryAddTransient(repo1, sp => sp.GetRequiredService(repo2));
        }
    }

    public static (Type AggregateType, Type KeyType) GetAggregateAndKeyTypeFromCreateCommand(this Type commandType) =>
        CreateCommandCache.GetOrAdd(commandType, static type =>
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Create<,>))
                {
                    return (current.GetGenericArguments()[0], current.GetGenericArguments()[1]);
                }
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Create<>))
                {
                    return (current.GetGenericArguments()[0], typeof(Guid));
                }
                current = current.BaseType;
            }

            var aggType = type.GetAggregateTypeFromCommand();
            var keyType = aggType.GetKeyTypeFromAggregate() ?? typeof(Guid);
            return (aggType, keyType);
        });

    public static (Type AggregateType, Type KeyType) GetAggregateAndKeyTypeFromChangeCommand(this Type commandType) =>
        ChangeCommandCache.GetOrAdd(commandType, static type =>
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Change<,>))
                {
                    return (current.GetGenericArguments()[0], current.GetGenericArguments()[1]);
                }
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Change<>))
                {
                    return (current.GetGenericArguments()[0], typeof(Guid));
                }
                current = current.BaseType;
            }

            var aggType = type.GetAggregateTypeFromCommand();
            var keyType = aggType.GetKeyTypeFromAggregate() ?? typeof(Guid);
            return (aggType, keyType);
        });

    public static Type? GetKeyTypeFromAggregate(this Type aggregateType) =>
        AggregateKeyTypeCache.GetOrAdd(aggregateType, static type =>
        {
            var aggInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAggregate<>));
            if (aggInterface != null)
                return aggInterface.GetGenericArguments()[0];

            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Aggregate<,>))
                    return current.GetGenericArguments()[1];
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Aggregate<>))
                    return typeof(Guid);
                current = current.BaseType;
            }

            return null;
        });

    public static Type GetAggregateTypeFromCommand(this Type commandType) =>
        CommandAggregateTypeCache.GetOrAdd(commandType, static type =>
        {
            var current = type;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    var def = current.GetGenericTypeDefinition();
                    if (def == typeof(Create<,>) || def == typeof(Create<>) ||
                        def == typeof(Change<,>) || def == typeof(Change<>))
                    {
                        return current.GetGenericArguments()[0];
                    }
                }
                current = current.BaseType;
            }

            var noHandlerCreate = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INoHandlerCreate<>));
            if (noHandlerCreate != null)
                return noHandlerCreate.GetGenericArguments()[0];

            var noHandlerChange = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INoHandlerChange<>));
            if (noHandlerChange != null)
                return noHandlerChange.GetGenericArguments()[0];

            throw new InvalidOperationException($"Could not determine Aggregate type from command {type.FullName}");
        });
}