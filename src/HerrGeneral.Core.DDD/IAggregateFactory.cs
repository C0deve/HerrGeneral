namespace HerrGeneral.DDD;

/// <summary>
/// Aggregate factory used by CreateHandlerDynamic to create an aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface IAggregateFactory<TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Create a new aggregate from a create command and an aggregate id
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public TAggregate Create(Create<TAggregate> command, Guid aggregateId);
}