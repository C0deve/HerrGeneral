using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public abstract class CommandHandler<TCommand> : CommandHandlerBase<TCommand, CommandResultV2>
    where TCommand : ICommand<CommandResultV2>
{
    protected CommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}