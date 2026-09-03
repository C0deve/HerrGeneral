using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace HerrGeneral.DDD;

// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// Default aggregate factory
/// Call new TAggregate(Create command, Guid aggregateId)
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public class DefaultAggregateFactory<TAggregate> : IAggregateFactory<TAggregate> where TAggregate : IAggregate
{
    private static readonly ConcurrentDictionary<Type, Func<Create<TAggregate>, Guid, TAggregate>> FactoryCache = new();

    /// <summary>
    /// Call new TAggregate(Create command, Guid aggregateId)
    /// The constructor can be private or internal
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throw InvalidOperationException if the constructor new TAggregate(Create command, Guid aggregateId) is not found</exception>
    public TAggregate Create(Create<TAggregate> command, Guid aggregateId)
    {
        var factory = FactoryCache.GetOrAdd(command.GetType(), CompileFactory);
        return factory(command, aggregateId);
    }

    private static Func<Create<TAggregate>, Guid, TAggregate> CompileFactory(Type commandType)
    {
        var constructors = typeof(TAggregate).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var ctor = constructors.FirstOrDefault(c =>
        {
            var parameters = c.GetParameters();
            return parameters.Length == 2
                   && parameters[0].ParameterType.IsAssignableFrom(commandType)
                   && parameters[1].ParameterType == typeof(Guid);
        });

        if (ctor is null)
        {
            return (_, _) => throw new MissingMethodException(
                $"Constructor new {typeof(TAggregate)}({commandType} command, {typeof(Guid)} aggregateId) not found.");
        }

        var commandParam = Expression.Parameter(typeof(Create<TAggregate>), "command");
        var aggregateIdParam = Expression.Parameter(typeof(Guid), "aggregateId");

        var ctorParamType = ctor.GetParameters()[0].ParameterType;
        Expression typedCommand = ctorParamType == typeof(Create<TAggregate>)
            ? commandParam
            : Expression.Convert(commandParam, ctorParamType);

        var newExpression = Expression.New(ctor, typedCommand, aggregateIdParam);
        return Expression.Lambda<Func<Create<TAggregate>, Guid, TAggregate>>(newExpression, commandParam, aggregateIdParam).Compile();
    }
}
