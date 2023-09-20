namespace HerrGeneral.WriteSide.DDD;

internal class CreateHandlerDynamic<TAggregate, TCommand> : CreateHandler<TAggregate, TCommand> where TAggregate : Aggregate<TAggregate> where TCommand : Create<TAggregate>
{
    private readonly IAggregateFactory<TAggregate> _aggregateFactory;

    public CreateHandlerDynamic(CtorParams @params, IAggregateFactory<TAggregate> aggregateFactory) : base(@params) => 
        _aggregateFactory = aggregateFactory;

    protected override TAggregate Handle(TCommand command, Guid aggregateId) => 
        _aggregateFactory.Create(command, aggregateId);
}
