using System.Text;
using HerrGeneral.WriteSide.DDD.Utils;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Command for editing an aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public record ChangeAggregate<TAggregate> : Command
    where TAggregate : Aggregate<TAggregate>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="aggregateId"></param>
    protected ChangeAggregate(Guid aggregateId) => AggregateId = aggregateId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <param name="executionDate"></param>
    protected ChangeAggregate(Guid aggregateId, DateTime executionDate) : base(executionDate) =>
        AggregateId = aggregateId;

    /// <summary>
    /// Id of the edited aggregate
    /// </summary>
    public Guid AggregateId { get; }
}
