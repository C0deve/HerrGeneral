namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record Ping : CommandBase
{
    public class Handler(CommandTracker1 commandTracker) : CommandHandler<Ping>
    {
        protected override IEnumerable<object> InnerHandle(Ping command)
        {
            commandTracker.AddHandled(command.Id);
            return [new Pong(command.Id, Guid.NewGuid())];
        }
    }
}