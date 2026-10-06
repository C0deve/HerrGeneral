namespace HerrGeneral.DDD;

/// <summary>
/// Handler for editing an aggregate with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IChangeHandler<TAggregate, in TCommand, TKey> 
    where TAggregate : Aggregate<TAggregate, TKey> 
    where TCommand : Change<TAggregate, TKey>
    where TKey : notnull
{
    /// <summary>
    /// Edit the aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    TAggregate Handle(TAggregate aggregate, TCommand command);
}

/// <summary>
/// Handler for editing an aggregate with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public interface IChangeHandler<TAggregate, in TCommand> : IChangeHandler<TAggregate, TCommand, Guid>
    where TAggregate : Aggregate<TAggregate, Guid> 
    where TCommand : Change<TAggregate, Guid>
{
}