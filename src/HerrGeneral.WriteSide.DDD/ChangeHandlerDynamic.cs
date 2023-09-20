using System.Reflection;

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
        (TAggregate)(typeof(TAggregate).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .SingleOrDefault(info => info.Name == "Execute" && info.GetParameters().Any(parameterInfo => parameterInfo.ParameterType == typeof(TCommand)))?
                         .Invoke(aggregate, new object?[] { command })
                     ?? throw new MissingMethodException($"{typeof(TAggregate)}.Execute({typeof(TCommand)} command) not found."));
}