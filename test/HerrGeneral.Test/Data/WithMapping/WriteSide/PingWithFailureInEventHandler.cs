namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PingWithFailureInEventHandler : CommandBase
{
    public class Handler : CommandHandler<PingWithFailureInEventHandler>
    {
        protected override IEnumerable<object> InnerHandle(PingWithFailureInEventHandler command) => 
            [new PongWithFailureInEventHandlerEvent(command.Id, Guid.NewGuid())];
    }
}