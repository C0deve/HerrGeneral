namespace HerrGeneral.DDD;

/// <summary>
/// Defines a cross-aggregate handler executed within the active transaction to modify another aggregate.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
/// <typeparam name="TAggregate">The aggregate type being modified.</typeparam>
public interface IHandleCrossAggregate<in TEvent, TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Handles the event and returns change requests to be applied to the target aggregate.
    /// </summary>
    /// <param name="event">The domain event being handled.</param>
    /// <returns>A collection of change requests for the target aggregate.</returns>
    ChangeRequests<TAggregate> Handle(TEvent @event);
}
