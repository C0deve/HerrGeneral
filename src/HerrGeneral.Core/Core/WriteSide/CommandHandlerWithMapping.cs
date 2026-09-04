using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Internal handler used to map client handler
/// </summary>
/// <param name="handler"></param>
/// <param name="mappingProvider"></param>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <typeparam name="TResult"></typeparam>
internal class CommandHandlerWithMapping<TCommand, THandler, TResult>(THandler handler, CommandHandlerMappings mappingProvider)
    : ICommandHandler<TCommand, TResult>, IHandlerTypeProvider
    where TCommand : notnull
    where THandler : notnull
{
    private static readonly ConcurrentDictionary<Type, Func<THandler, TCommand, object>> InvokerCache = new();

    public (IReadOnlyList<object> Events, TResult Result) Handle(TCommand command)
    {
        var mapping = mappingProvider.GetFromCommand(command, typeof(TResult));

        var invoker = InvokerCache.GetOrAdd(command.GetType(), _ =>
        {
            var handleMethod = typeof(THandler).GetMethod(mapping.MethodInfo.Name) 
                               ?? throw new InvalidOperationException();
            return CompileInvoker(handleMethod);
        });

        object result;
        try
        {
            result = invoker(handler, command) ?? throw new InvalidOperationException();
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }

        try
        {
            var events = mapping.MapEvents(result);
            dynamic value =
                mapping.MapValue is null
                    ? Unit.Default
                    : Convert.ChangeType(mapping.MapValue(result), typeof(TResult));

            return (events, value);
        }
        catch (System.Exception e)
        {
            var mappingHandlerType = mapping.MethodInfo.DeclaringType!;
            throw new ConversionException(result.GetType(), mappingHandlerType, e);
        }
    }

    public Type GetHandlerType() => typeof(THandler);

    private static Func<THandler, TCommand, object> CompileInvoker(MethodInfo methodInfo)
    {
        var handlerParam = Expression.Parameter(typeof(THandler), "handler");
        var commandParam = Expression.Parameter(typeof(TCommand), "command");

        var methodParamType = methodInfo.GetParameters()[0].ParameterType;
        Expression typedCommand = methodParamType == typeof(TCommand)
            ? commandParam
            : Expression.Convert(commandParam, methodParamType);

        var call = Expression.Call(handlerParam, methodInfo, typedCommand);
        Expression body = methodInfo.ReturnType == typeof(void)
            ? Expression.Block(call, Expression.Constant(null, typeof(object)))
            : Expression.Convert(call, typeof(object));

        return Expression.Lambda<Func<THandler, TCommand, object>>(body, handlerParam, commandParam).Compile();
    }
}
