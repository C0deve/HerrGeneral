using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD;

/// <summary>
/// Command for editing an aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public record Change<TAggregate>(Guid AggregateId) : CommandBase, IKeyedCommand
    where TAggregate : IAggregate
{
    /// <summary>
    /// Key used for partitioned concurrency locking.
    /// </summary>
    public virtual object Key => (typeof(TAggregate), (object)AggregateId);
}
