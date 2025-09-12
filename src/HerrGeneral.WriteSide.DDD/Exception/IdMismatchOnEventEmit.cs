namespace HerrGeneral.DDD.Exception;

/// <summary>
/// The aggregate id of the event is different from aggregate id of the emitter
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public class IdMismatchOnEventEmit<TAggregate> : System.Exception where TAggregate : Aggregate<TAggregate>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="event"></param>
    public IdMismatchOnEventEmit(Aggregate<TAggregate> aggregate, IDomainEvent<TAggregate> @event)
        : base($"{typeof(TAggregate)} attempt to issue {@event.GetType()} with an AggregateId<{@event.AggregateId}> different from it's own Id<{aggregate.Id}>.")
    {
    }
}