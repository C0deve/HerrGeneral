namespace HerrGeneral.DDD;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public interface IChangeHandler<TAggregate, in TCommand> 
    where TAggregate : Aggregate<TAggregate> 
    where TCommand : Change<TAggregate>
{
    /// <summary>
    /// Edit the aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    TAggregate Handle(TAggregate aggregate, TCommand command);
}