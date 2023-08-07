using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingWithFailureInEventHandler : Command
{
    public class Handler : CommandHandler<PingWithFailureInEventHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResult> Handle(PingWithFailureInEventHandler command, CancellationToken cancellationToken)
        {
            await Publish(new PongWithFailureInEventHandlerEvent(command.Id, Guid.NewGuid()), cancellationToken);
            return CommandResult.Success;
        }
    }
}