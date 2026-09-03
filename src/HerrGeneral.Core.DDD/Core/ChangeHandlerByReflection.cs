using System.Linq.Expressions;
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
    private static readonly Func<TAggregate, TCommand, TAggregate> Handler = CompileHandler();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException"></exception>
    public TAggregate Handle(TAggregate aggregate, TCommand command) => Handler(aggregate, command);

    private static Func<TAggregate, TCommand, TAggregate> CompileHandler()
    {
        var methodInfo = typeof(TAggregate)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SingleOrDefault(info => info.Name == "Execute" && info.GetParameters().Count(parameterInfo => parameterInfo.ParameterType == typeof(TCommand)) == 1);

        if (methodInfo is null)
        {
            return (_, _) => throw new MissingMethodException($"{typeof(TAggregate)}.Execute({typeof(TCommand)} command) not found.");
        }

        var aggregateParam = Expression.Parameter(typeof(TAggregate), "aggregate");
        var commandParam = Expression.Parameter(typeof(TCommand), "command");
        var call = Expression.Call(aggregateParam, methodInfo, commandParam);

        Expression body = methodInfo.ReturnType == typeof(void)
            ? Expression.Block(call, aggregateParam)
            : Expression.Convert(call, typeof(TAggregate));

        return Expression.Lambda<Func<TAggregate, TCommand, TAggregate>>(body, aggregateParam, commandParam).Compile();
    }
}
