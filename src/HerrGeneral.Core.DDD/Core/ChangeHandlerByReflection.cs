using System.Reflection;

namespace HerrGeneral.DDD.Core;

/// <summary>
/// Handle a change by calling the aggregate method that accepts that command
/// Allows you to avoid declaring a handler for the command
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
internal sealed class ChangeHandlerByReflection<TAggregate, TCommand> : IChangeHandler<TAggregate, TCommand>
    where TAggregate : Aggregate<TAggregate> 
    where TCommand : Change<TAggregate>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    public TAggregate Handle(TAggregate aggregate, TCommand command) => 
        (TAggregate)(typeof(TAggregate).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .SingleOrDefault(info => info.Name == "Execute" && info.GetParameters().Count(parameterInfo => parameterInfo.ParameterType == typeof(TCommand)) == 1)?
                         .Invoke(aggregate, [command])
                     ?? throw new MissingMethodException($"{typeof(TAggregate)}.Execute({typeof(TCommand)} command) not found."));
}