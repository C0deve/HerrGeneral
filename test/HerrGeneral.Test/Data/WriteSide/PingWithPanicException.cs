using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingWithPanicException : Command
{
    public class Handler : CommandHandler<PingWithPanicException>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResultV2> Handle(PingWithPanicException command, CancellationToken cancellationToken)
        {
            await Publish(new Pong("Command received", command.Id, Guid.NewGuid()), cancellationToken);
            throw new PanicException();
        }
    }
}