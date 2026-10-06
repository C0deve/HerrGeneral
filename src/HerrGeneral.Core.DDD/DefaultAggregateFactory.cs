using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace HerrGeneral.DDD;

// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// Default aggregate factory with typed key
/// Call new TAggregate(Create command, TKey aggregateId)
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class DefaultAggregateFactory<TAggregate, TKey> : IAggregateFactory<TAggregate, TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    private static readonly ConcurrentDictionary<Type, Func<Create<TAggregate, TKey>, TKey, TAggregate>> FactoryCache = new();

    /// <summary>
    /// Call new TAggregate(Create command, TKey aggregateId)
    /// The constructor can be private or internal
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throw InvalidOperationException if the constructor new TAggregate(Create command, TKey aggregateId) is not found</exception>
    public TAggregate Create(Create<TAggregate, TKey> command, TKey aggregateId)
    {
        var factory = FactoryCache.GetOrAdd(command.GetType(), CompileFactory);
        return factory(command, aggregateId);
    }

    private static Func<Create<TAggregate, TKey>, TKey, TAggregate> CompileFactory(Type commandType)
    {
        var constructors = typeof(TAggregate).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var ctor = constructors.FirstOrDefault(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                   && parameters[0].ParameterType.IsAssignableFrom(commandType)
                   && parameters[1].ParameterType == typeof(TKey);
        });

        if (ctor is null)
        {
            return (_, _) => throw new MissingMethodException(
                $"Constructor new {typeof(TAggregate)}({commandType} command, {typeof(TKey)} aggregateId) not found.");
        }

        var commandParam = Expression.Parameter(typeof(Create<TAggregate, TKey>), "command");
        var aggregateIdParam = Expression.Parameter(typeof(TKey), "aggregateId");

        var ctorParamType = ctor.GetParameters()[0].ParameterType;
        Expression typedCommand = ctorParamType == typeof(Create<TAggregate, TKey>)
            ? commandParam
            : Expression.Convert(commandParam, ctorParamType);

        var newExpression = Expression.New(ctor, typedCommand, aggregateIdParam);
        return Expression.Lambda<Func<Create<TAggregate, TKey>, TKey, TAggregate>>(newExpression, commandParam, aggregateIdParam).Compile();
    }
}

/// <summary>
/// Default aggregate factory with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public class DefaultAggregateFactory<TAggregate> : DefaultAggregateFactory<TAggregate, Guid>, IAggregateFactory<TAggregate>
    where TAggregate : IAggregate<Guid>
{
}
