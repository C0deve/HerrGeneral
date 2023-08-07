namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for command returning a CommandResultV2
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CommandHandler<TCommand> : CommandHandlerBase<TCommand, CommandResult>
    where TCommand : Command
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="eventDispatcher"></param>
    protected CommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}