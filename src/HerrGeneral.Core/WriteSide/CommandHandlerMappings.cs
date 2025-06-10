using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Register mapping between client command handler and internal <see cref="ICommandHandler{TCommand,TResult}"/>*
/// Used by internal CommandHandler to return ((IEnumerable{object Events, TResult Result)}) from a client handler
/// </summary>
internal class CommandHandlerMappings
{
    private readonly Dictionary<(Type TCommand, Type TResult), CommandHandlerMapping> _handlerMappers = new();

    public CommandHandlerMappings AddMapping<TCommand, THandler, THandlerReturn, TValue>(
        Func<THandlerReturn, IEnumerable<object>> mapEvents,
        Func<THandlerReturn, TValue>? mapValue) where TValue : notnull
    {
        var handlerType = typeof(THandler);

        var methodInfo = handlerType.FindMethodWithUniqueParameterOfType(typeof(TCommand))
                         ?? throw new NotSupportedException($"No method that accepts a unique parameter of type '{typeof(TCommand)}' was found on type '{typeof(THandler).Name}'");

        var returnType = methodInfo.ReturnType;
        if (returnType != typeof(THandlerReturn))
            throw new TypeMismatchInMappingDefinitionException(typeof(THandlerReturn), methodInfo);

        if(! handlerType.IsGenericType)
            throw new HandlerTypeMustBeGenericMappingDefinitionException(handlerType);
        
        Func<object, object>? mapValueFinal =
            mapValue is null
            ? null
            : o => mapValue((THandlerReturn)o);
        
        _handlerMappers.Add(
            (typeof(TCommand), typeof(TValue)),
            new CommandHandlerMapping(
                methodInfo,
                handlerType.GetGenericTypeDefinition(),
                typeof(TValue),
                o => mapEvents((THandlerReturn)o),
                mapValueFinal
            )
        );

        return this;
    }

    public CommandHandlerMappings AddMapping<TCommand, THandler, THandlerReturn>(
        Func<THandlerReturn, IEnumerable<object>> mapEvents) =>
        AddMapping<TCommand, THandler, THandlerReturn, Unit>(mapEvents, _ => Unit.Default);
   
    /// <summary>
    /// Return handlerMapper corresponding to <see cref="command"/>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="returnType"></param>
    /// <returns></returns>
    public CommandHandlerMapping GetFromCommand(object command, Type? returnType = null)
    {
        returnType ??= typeof(Unit);
        foreach (var key in _handlerMappers.Keys.Reverse())
        {
            if (returnType == key.TResult && command.GetType().IsAssignableTo(key.TCommand))
                return _handlerMappers[key];
        }

        throw new MissingCommandHandlerMapperException(command.GetType(), returnType);
    }
    
    public IEnumerable<CommandHandlerMapping> All() => _handlerMappers.Values;
}