using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInCommandHandler
{
    public class Handler : CommandHandler<PingWithFailureInCommandHandler>
    {
        protected override IEnumerable<object> Handle(PingWithFailureInCommandHandler command) => 
            throw new PingError().ToException();
    }
}