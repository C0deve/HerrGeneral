namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Processes a domain event and returns aggregates that have been modified or created as a result.
/// This method implements the event sourcing pattern where events drive state changes in aggregates.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="TAggregate"></typeparam>
public interface IEventHandler<in TEvent, out TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Processes a domain event and returns aggregates that have been modified or created as a result.
    /// </summary>
    /// <param name="notification">The domain event to process. Must not be null.</param>
    /// <returns>
    /// A collection of aggregates that have been affected by this event.
    /// May be empty if the event doesn't result in any aggregate changes.
    /// Each aggregate may contain new domain events that need to be dispatched.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null</exception>
    /// <exception cref="DomainException">Thrown when the event cannot be processed due to business rule violations</exception>

    IEnumerable<TAggregate> Handle(TEvent notification);
}