namespace HerrGeneral.WriteSide.DDD;

internal class CreateHandlerDynamic<TAggregate, TCommand> : CreateHandler<TAggregate, TCommand> where TAggregate : Aggregate<TAggregate> where TCommand : Create<TAggregate>
{
    public CreateHandlerDynamic(CtorParams @params) : base(@params)
    {
    }

    protected override TAggregate Handle(TCommand command, Guid aggregateId) =>
        (TAggregate)(Activator.CreateInstance(typeof(TAggregate), command, aggregateId) ?? throw new InvalidOperationException());
}