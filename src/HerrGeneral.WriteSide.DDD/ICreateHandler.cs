namespace HerrGeneral.DDD;

/// <summary>
/// Contract for an handler that creates aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public interface ICreateHandler<out TAggregate, in TCommand> 
    where TAggregate : Aggregate<TAggregate> 
    where TCommand : Create<TAggregate>
{
    /// <summary>
    /// Create an aggregate from command and id
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    TAggregate Handle(TCommand command, Guid aggregateId);
}