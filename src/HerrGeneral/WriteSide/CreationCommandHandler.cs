using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler for creation command
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreationCommandHandler<TCommand> : CommandHandlerBase<TCommand, CreationResult>
    where TCommand : ICommand<CreationResult>
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="eventDispatcher"></param>
    protected CreationCommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}