using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInEventHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInEventHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<ChangeResult> Handle(PingWithFailureInEventHandler command, CancellationToken cancellationToken)
        {
            await Publish(new PongWithFailureInEventHandlerEvent(command.Id, Guid.NewGuid()), cancellationToken);
            return ChangeResult.Success;
        }
    }
}