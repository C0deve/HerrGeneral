using HerrGeneral.Contracts;
using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.WriteSide;

public abstract class CommandHandler<TCommand> : CommandHandlerBase<TCommand, CommandResultV2>
    where TCommand : ICommand<CommandResultV2>
{
    protected CommandHandler(IEventDispatcher eventDispatcher) : base(eventDispatcher) { }
}