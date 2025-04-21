namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for creation command returning a CreationResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreateHandler<TCommand> : ICommandHandler<TCommand, Guid>
    where TCommand : Create
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected CreateHandler()
    {
    }

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract (IEnumerable<object> Events, Guid Result) Handle(TCommand command, CancellationToken cancellationToken);
}