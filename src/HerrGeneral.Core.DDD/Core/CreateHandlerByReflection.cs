namespace HerrGeneral.DDD.Core;

/// <summary>
/// Handle an aggregate creation using <see cref="IAggregateFactory{TAggregate, TKey}"/>
/// Allows to avoid declaring a handler
/// </summary>
/// <param name="aggregateFactory"></param>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TCommand"></typeparam>
internal sealed class CreateHandlerByReflection<TAggregate, TKey, TCommand>(IAggregateFactory<TAggregate, TKey> aggregateFactory)
    : ICreateHandler<TAggregate, TCommand, TKey>
    where TAggregate : Aggregate<TAggregate, TKey>
    where TCommand : Create<TAggregate, TKey>
    where TKey : notnull
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public TAggregate Handle(TCommand command, TKey aggregateId) =>
        aggregateFactory.Create(command, aggregateId);
}