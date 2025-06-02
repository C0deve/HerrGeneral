namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInCommandHandler : CommandBase
{
    public class Handler : CommandHandler<PingWithFailureInCommandHandler>
    {
        protected override IEnumerable<object> InnerHandle(PingWithFailureInCommandHandler command) => 
            throw new PingError();
    }
}