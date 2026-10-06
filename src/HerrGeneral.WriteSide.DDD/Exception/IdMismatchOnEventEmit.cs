namespace HerrGeneral.DDD.Exception;

/// <summary>
/// The aggregate id of the event is different from aggregate id of the emitter with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="aggregate"></param>
/// <param name="event"></param>
public class IdMismatchOnEventEmit<TAggregate, TKey>(Aggregate<TAggregate, TKey> aggregate, IDomainEvent<TAggregate, TKey> @event) : System.Exception($"{typeof(TAggregate)} attempt to issue {@event.GetType()} with an AggregateId<{@event.AggregateId}> different from it's own Id<{aggregate.Id}>.")
    where TAggregate : Aggregate<TAggregate, TKey>
    where TKey : notnull
{
}

/// <summary>
/// The aggregate id of the event is different from aggregate id of the emitter with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="aggregate"></param>
/// <param name="event"></param>
public class IdMismatchOnEventEmit<TAggregate>(Aggregate<TAggregate> aggregate, IDomainEvent<TAggregate> @event) : IdMismatchOnEventEmit<TAggregate, Guid>(aggregate, @event)
    where TAggregate : Aggregate<TAggregate>
{
}