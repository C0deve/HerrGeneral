using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInCommandHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInCommandHandler>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(PingWithFailureInCommandHandler command,
            CancellationToken cancellationToken) => 
            throw new PingError().ToException();
    }
}