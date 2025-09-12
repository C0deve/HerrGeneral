namespace HerrGeneral.DDD.Core;

/// <summary>
/// Handle an aggregate creation using <see cref="IAggregateFactory{TAggregate}"/>
/// Allows to avoid declaring a handler
/// </summary>
/// <param name="aggregateFactory"></param>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
internal sealed class CreateHandlerByReflection<TAggregate, TCommand>(IAggregateFactory<TAggregate> aggregateFactory)
    : ICreateHandler<TAggregate, TCommand>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Create<TAggregate>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public TAggregate Handle(TCommand command, Guid aggregateId) =>
        aggregateFactory.Create(command, aggregateId);
}