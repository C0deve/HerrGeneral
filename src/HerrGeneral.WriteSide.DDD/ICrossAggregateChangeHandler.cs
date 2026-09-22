namespace HerrGeneral.DDD;

/// <summary>
/// Represents a handler that processes a specific event across aggregates and determines change requests for a target aggregate type.
/// </summary>
/// <typeparam name="TEvent">The type of the event to be handled.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregate that will be affected by the change requests.</typeparam>
public interface ICrossAggregateChangeHandler<in TEvent, TAggregate> : IHandleCrossAggregate<TEvent, TAggregate> where TAggregate : IAggregate
{
}