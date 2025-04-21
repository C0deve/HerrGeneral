using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInReadSideEventHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInReadSideEventHandler>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(PingWithFailureInReadSideEventHandler command, CancellationToken cancellationToken) => 
            ([new PongWithReadSideFailure(command.Id, Guid.NewGuid())], Unit.Default);
    }
}