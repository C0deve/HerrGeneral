namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for creation command returning a CreationResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreateHandler<TCommand> : CommandHandlerBase<TCommand, CreateResult>
    where TCommand : Create
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="eventDispatcher"></param>
    protected CreateHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}