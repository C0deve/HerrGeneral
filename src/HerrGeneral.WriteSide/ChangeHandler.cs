namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for command returning a CommandResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class ChangeHandler<TCommand> : CommandHandlerBase<TCommand, ChangeResult>
    where TCommand : Change
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="eventDispatcher"></param>
    protected ChangeHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}