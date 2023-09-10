using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInReadSideEventHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInReadSideEventHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<ChangeResult> Handle(PingWithFailureInReadSideEventHandler command, CancellationToken cancellationToken)
        {
            await Publish(new PongWithReadSideFailure(command.Id, Guid.NewGuid()), cancellationToken);
            return ChangeResult.Success;
        }
    }
}