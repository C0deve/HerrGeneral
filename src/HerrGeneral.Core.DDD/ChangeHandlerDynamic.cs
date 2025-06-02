using System.Reflection;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

internal class ChangeHandlerDynamic<TAggregate, TCommand> : ChangeHandler<TAggregate, TCommand> , ICommandHandler<TCommand, Unit>
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
                         .SingleOrDefault(info => info.Name == "Execute" && info.GetParameters().Count(parameterInfo => parameterInfo.ParameterType == typeof(TCommand)) == 1)?
                         .Invoke(aggregate, [command])
                     ?? throw new MissingMethodException($"{typeof(TAggregate)}.Execute({typeof(TCommand)} command) not found."));

    public new (IEnumerable<object> Events, Unit Result) Handle(TCommand command) => 
        (base.Handle(command), Unit.Default);
}