namespace HerrGeneral.Test.Data.WriteSide;

public record Ping : CommandBase
{
    public string Message { get; init; } = string.Empty;

    public class Handler : CommandHandler<Ping>
    {
        public static Pong? LastPong { get; private set; }

        protected override IEnumerable<object> InnerHandle(Ping command)
        {
            LastPong = new Pong($"{command.Message} received", command.Id, Guid.NewGuid());
            return [LastPong];
        }
    }
}