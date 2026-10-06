namespace HerrGeneral.DDD;

/// <summary>
/// Aggregate factory used by CreateHandlerDynamic to create an aggregate with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IAggregateFactory<TAggregate, TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Create a new aggregate from a create command and an aggregate id
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public TAggregate Create(Create<TAggregate, TKey> command, TKey aggregateId);
}

/// <summary>
/// Aggregate factory with Guid key for backwards compatibility
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IAggregateFactory<TAggregate> : IAggregateFactory<TAggregate, Guid>
    where TAggregate : IAggregate<Guid>
{
}