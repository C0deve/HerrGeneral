using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingWithFailureInCommandHandler : Command
{
    public class Handler : CommandHandler<PingWithFailureInCommandHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResultV2> Handle(PingWithFailureInCommandHandler command, CancellationToken cancellationToken)
        {
            await Publish(new Pong("Command received", command.Id, Guid.NewGuid()), cancellationToken);
            throw new PingError().ToException();
        }
    }
}