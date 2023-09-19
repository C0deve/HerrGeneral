namespace HerrGeneral.WriteSide.DDD;

internal class CreateAggregateHandlerDynamic<TAggregate, TCommand> : CreateAggregateHandler<TAggregate, TCommand> where TAggregate : Aggregate<TAggregate> where TCommand : CreateAggregate<TAggregate>
{
    public CreateAggregateHandlerDynamic(CtorParams @params) : base(@params)
    {
    }

    protected override TAggregate Handle(TCommand command, Guid aggregateId) =>
        (TAggregate)(Activator.CreateInstance(typeof(TAggregate), command, aggregateId) ?? throw new InvalidOperationException());
}