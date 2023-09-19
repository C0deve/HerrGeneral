namespace HerrGeneral.WriteSide.DDD;

internal class ChangeHandlerDynamic<TAggregate, TCommand> : ChangeHandler<TAggregate, TCommand> 
    where TAggregate : Aggregate<TAggregate> where TCommand : Change<TAggregate>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ctorParams"></param>
    public ChangeHandlerDynamic(CtorParams ctorParams) : base(ctorParams)
    {
    }

    protected sealed override TAggregate Handle(TAggregate aggregate, TCommand command) => 
        ((dynamic)aggregate).Execute(command);
}