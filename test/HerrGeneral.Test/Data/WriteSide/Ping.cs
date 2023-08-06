using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class Ping : Command
{
    public string Message { get; set; } = string.Empty;

    public class Handler : CommandHandler<Ping>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResultV2> Handle(Ping command, CancellationToken cancellationToken)
        {
            await Publish(new Pong($"{command.Message} received", command.Id, Guid.NewGuid()), cancellationToken);
            return CommandResultV2.Success;
        }
    }
}