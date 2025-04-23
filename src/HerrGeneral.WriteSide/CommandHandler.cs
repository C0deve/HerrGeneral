namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for command returning a CommandResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand, Unit>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected CommandHandler()
    {
    }

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public (IEnumerable<object> Events, Unit Result) Handle(TCommand command, CancellationToken cancellationToken) 
        => (Handle(command), Unit.Default);

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    protected abstract IEnumerable<object> Handle(TCommand command);
}

/// <summary>
/// Handler for command returning a value of type <see cref="TResult"/>
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public abstract class CommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected CommandHandler()
    {
    }

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract (IEnumerable<object> Events, TResult Result) Handle(TCommand command, CancellationToken cancellationToken);
}