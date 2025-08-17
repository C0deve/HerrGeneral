namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record Ping : CommandBase
{
    public class Handler(EventTracker eventTracker) : CommandHandler<Ping>
    {
        protected override IEnumerable<object> InnerHandle(Ping command)
        {
            var pong = new Pong(command.Id, Guid.NewGuid());
            eventTracker.AddHandled(pong);
            return [pong];
        }
    }
}