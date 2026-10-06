namespace HerrGeneral.DDD;

/// <summary>
/// Contract for a handler that creates aggregate with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface ICreateHandler<out TAggregate, in TCommand, in TKey> 
    where TAggregate : Aggregate<TAggregate, TKey> 
    where TCommand : Create<TAggregate, TKey>
    where TKey : notnull
{
    /// <summary>
    /// Create an aggregate from command and id
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    TAggregate Handle(TCommand command, TKey aggregateId);
}

/// <summary>
/// Contract for a handler that creates aggregate with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public interface ICreateHandler<out TAggregate, in TCommand> : ICreateHandler<TAggregate, TCommand, Guid>
    where TAggregate : Aggregate<TAggregate, Guid> 
    where TCommand : Create<TAggregate, Guid>
{
}