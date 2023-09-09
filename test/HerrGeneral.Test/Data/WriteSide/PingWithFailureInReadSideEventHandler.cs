using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingWithFailureInReadSideEventHandler : Command
{
    public class Handler : CommandHandler<PingWithFailureInReadSideEventHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResult> Handle(PingWithFailureInReadSideEventHandler command, CancellationToken cancellationToken)
        {
            await Publish(new PongWithReadSideFailure(command.Id, Guid.NewGuid()), cancellationToken);
            return CommandResult.Success;
        }
    }
}