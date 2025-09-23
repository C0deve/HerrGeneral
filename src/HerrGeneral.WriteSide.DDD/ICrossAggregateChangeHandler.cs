namespace HerrGeneral.DDD;

/// <summary>
/// Represents a handler that processes a specific event across aggregates and determines change requests for a target aggregate type.
/// </summary>
/// <typeparam name="TEvent">The type of the event to be handled.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate that will be affected by the change requests.</typeparam>
public interface ICrossAggregateChangeHandler<in TEvent, TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Handles the given event and determines the set of change requests for the corresponding aggregate type.
    /// </summary>
    /// <param name="notification">The event or notification to be processed, which contains information relevant for determining changes.</param>
    /// <returns>
    /// A collection of change requests that specify modifications to be applied to the target aggregate type.
    /// </returns>
    ChangeRequests<TAggregate> Handle(TEvent notification);
}