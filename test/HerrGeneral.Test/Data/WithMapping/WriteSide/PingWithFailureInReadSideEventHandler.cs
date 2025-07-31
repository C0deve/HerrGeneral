namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PingWithFailureInReadSideEventHandler : CommandBase
{
    public class Handler : CommandHandler<PingWithFailureInReadSideEventHandler>
    {
        protected override IEnumerable<object> InnerHandle(PingWithFailureInReadSideEventHandler command) =>
            [new PongWithReadSideFailure(command.Id, Guid.NewGuid())];
    }
}