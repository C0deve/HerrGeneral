using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithPanicException : Command
{
    public class Handler : CommandHandler<PingWithPanicException>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResult> Handle(PingWithPanicException command, CancellationToken cancellationToken)
        {
            await Publish(new Pong("Command received", command.Id, Guid.NewGuid()), cancellationToken);
            throw new SomePanicException();
        }
    }
}