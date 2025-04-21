using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInEventHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInEventHandler>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(PingWithFailureInEventHandler command, CancellationToken cancellationToken) => 
            ([new PongWithFailureInEventHandlerEvent(command.Id, Guid.NewGuid())], Unit.Default);
    }
}