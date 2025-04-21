using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record Ping : Change
{
    public string Message { get; set; } = string.Empty;

    public class Handler : ChangeHandler<Ping>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(Ping command, CancellationToken cancellationToken) => 
            ([new Pong($"{command.Message} received", command.Id, Guid.NewGuid())], Unit.Default);
    }
}