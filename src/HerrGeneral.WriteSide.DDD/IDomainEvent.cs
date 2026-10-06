namespace HerrGeneral.DDD;

/// <summary>
/// Interface for all domain event with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IDomainEvent<TAggregate, out TKey>
    where TAggregate : IAggregate
    where TKey : notnull
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
    public TKey AggregateId { get; }
}

/// <summary>
/// Interface for domain event with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IDomainEvent<TAggregate> : IDomainEvent<TAggregate, Guid>
    where TAggregate : IAggregate
{
}