namespace HerrGeneral.DDD;

/// <summary>
/// Processes a domain event to create or modify aggregates.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="TAggregate"></typeparam>
public interface IDomainEventHandler<in TEvent, out TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Processes a domain event and returns aggregates that have been modified or created as a result.
    /// </summary>
    /// <param name="notification">The domain event to process.</param>
    /// <returns>
    /// A collection of aggregates that have been affected by this event.
    /// HerrGeneral will save each aggregate and dispatch their new domain events.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null</exception>
    /// <exception>Thrown when the event cannot be processed due to business rule violations</exception>
    IEnumerable<TAggregate> Handle(TEvent notification);
}