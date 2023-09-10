using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record Ping : Change
{
    public string Message { get; set; } = string.Empty;

    public class Handler : ChangeHandler<Ping>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<ChangeResult> Handle(Ping command, CancellationToken cancellationToken)
        {
            await Publish(new Pong($"{command.Message} received", command.Id, Guid.NewGuid()), cancellationToken);
            return ChangeResult.Success;
        }
    }
}