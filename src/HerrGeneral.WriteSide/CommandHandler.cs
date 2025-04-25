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
    /// <returns></returns>
    public (IEnumerable<object> Events, Unit Result) Handle(TCommand command) 
        => (InnerHandle(command), Unit.Default);

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    protected abstract IEnumerable<object> InnerHandle(TCommand command);
}