namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for creation command returning a CreationResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreationCommandHandler<TCommand> : CommandHandlerBase<TCommand, CreationResult>
    where TCommand : CreationCommand
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="eventDispatcher"></param>
    protected CreationCommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}