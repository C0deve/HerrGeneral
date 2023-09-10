using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record Ping : Command
{
    public string Message { get; set; } = string.Empty;

    public class Handler : CommandHandler<Ping>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CommandResult> Handle(Ping command, CancellationToken cancellationToken)
        {
            await Publish(new Pong($"{command.Message} received", command.Id, Guid.NewGuid()), cancellationToken);
            return CommandResult.Success;
        }
    }
}