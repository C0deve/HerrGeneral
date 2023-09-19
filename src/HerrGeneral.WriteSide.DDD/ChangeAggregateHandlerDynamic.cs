namespace HerrGeneral.WriteSide.DDD;

internal class ChangeAggregateHandlerDynamic<TAggregate, TCommand> : ChangeAggregateHandler<TAggregate, TCommand> 
    where TAggregate : Aggregate<TAggregate> where TCommand : ChangeAggregate<TAggregate>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ctorParams"></param>
    public ChangeAggregateHandlerDynamic(CtorParams ctorParams) : base(ctorParams)
    {
    }

    protected sealed override TAggregate Handle(TAggregate aggregate, TCommand command) => 
        ((dynamic)aggregate).Execute(command);
}