namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for command returning a CommandResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class ChangeHandler<TCommand> : ICommandHandler<TCommand, Unit> where TCommand : Change
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected ChangeHandler()
    {
    }

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract (IEnumerable<object> Events, Unit Result) Handle(TCommand command, CancellationToken cancellationToken);
}