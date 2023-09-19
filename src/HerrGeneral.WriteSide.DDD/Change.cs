using System.Text;
using HerrGeneral.WriteSide.DDD.Utils;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Command for editing an aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public record Change<TAggregate> : Change
    where TAggregate : IAggregate
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="aggregateId"></param>
    protected Change(Guid aggregateId) => AggregateId = aggregateId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <param name="executionDate"></param>
    protected Change(Guid aggregateId, DateTime executionDate) : base(executionDate) =>
        AggregateId = aggregateId;

    /// <summary>
    /// Id of the edited aggregate
    /// </summary>
    public Guid AggregateId { get; }
}
