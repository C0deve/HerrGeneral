using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record Ping : CommandBase
{
    public string Message { get; init; } = string.Empty;

    public class Handler : CommandHandler<Ping>
    {
        protected override IEnumerable<object> Handle(Ping command) => 
            [new Pong($"{command.Message} received", command.Id, Guid.NewGuid())];
    }
}