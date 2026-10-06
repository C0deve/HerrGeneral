using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD;

/// <summary>
/// Interface for change command with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IChange<TAggregate, out TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Id of the target aggregate
    /// </summary>
    TKey AggregateId { get; }
}

/// <summary>
/// Interface for change command with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IChange<TAggregate> : IChange<TAggregate, Guid>
    where TAggregate : IAggregate<Guid>
{
}

/// <summary>
/// Command for editing an aggregate with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract record Change<TAggregate, TKey>(TKey AggregateId) : CommandBase, IKeyedCommand, IChange<TAggregate, TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Key used for partitioned concurrency locking.
    /// </summary>
    public virtual object Key { get; } = (typeof(TAggregate).FullName ?? throw new InvalidOperationException(), (object)AggregateId);
}

/// <summary>
/// Command for editing an aggregate with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public abstract record Change<TAggregate>(Guid AggregateId) : Change<TAggregate, Guid>(AggregateId), IChange<TAggregate>
    where TAggregate : IAggregate<Guid>;
