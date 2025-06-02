using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

internal class CreateHandlerDynamic<TAggregate, TCommand>(
    CreateHandler<TAggregate, TCommand>.CtorParams @params,
    IAggregateFactory<TAggregate> aggregateFactory)
    : CreateHandler<TAggregate, TCommand>(@params), ICommandHandler<TCommand, Guid>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Create<TAggregate>
{
    protected override TAggregate Handle(TCommand command, Guid aggregateId) => 
        aggregateFactory.Create(command, aggregateId);
}
