using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInReadSideEventHandler : CommandBase
{
    public class Handler : CommandHandler<PingWithFailureInReadSideEventHandler>
    {
        protected override IEnumerable<object> Handle(PingWithFailureInReadSideEventHandler command) =>
            [new PongWithReadSideFailure(command.Id, Guid.NewGuid())];
    }
}