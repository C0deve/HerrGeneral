using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInEventHandler : CommandBase
{
    public class Handler : CommandHandler<PingWithFailureInEventHandler>
    {
        protected override IEnumerable<object> Handle(PingWithFailureInEventHandler command) => 
            [new PongWithFailureInEventHandlerEvent(command.Id, Guid.NewGuid())];
    }
}