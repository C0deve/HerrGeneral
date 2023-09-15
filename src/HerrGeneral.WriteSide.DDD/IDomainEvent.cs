using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Interface for all domain event
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IDomainEvent<TAggregate> : IEvent where TAggregate : IAggregate
{
    /// <summary>
    /// Id of the aggregate who produce the event
    /// </summary>
    public Guid AggregateId { get; }
}