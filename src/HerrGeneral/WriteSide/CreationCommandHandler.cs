using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public abstract class CreationCommandHandler<TCommand> : CommandHandlerBase<TCommand, CreationResult>
    where TCommand : ICommand<CreationResult>
{
    protected CreationCommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}