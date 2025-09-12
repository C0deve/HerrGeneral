namespace HerrGeneral.DDD;

/// <summary>
/// Processes a domain event.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IVoidDomainEventHandler<in TEvent>
{
    /// <summary>
    /// Processes a domain event.
    /// </summary>
    /// <param name="notification">The domain event to process.</param>
    /// <returns>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null</exception>
    /// <exception>Thrown when the event cannot be processed due to business rule violations</exception>
    void Handle(TEvent notification);
}