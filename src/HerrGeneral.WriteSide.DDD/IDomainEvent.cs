namespace HerrGeneral.DDD;

/// <summary>
/// Interface for all domain event
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IDomainEvent<TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Date of the event
    /// </summary>
    DateTime DateTimeEventOccurred { get; }
    
    /// <summary>
    /// Id of the event
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Id of the command at the origin of the event
    /// </summary>
    Guid SourceCommandId { get; }
    
    /// <summary>
    /// Id of the aggregate who produce the event
    /// </summary>
    public Guid AggregateId { get; }
}