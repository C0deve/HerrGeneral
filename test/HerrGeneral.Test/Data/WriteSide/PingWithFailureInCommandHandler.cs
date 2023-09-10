using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithFailureInCommandHandler : Change
{
    public class Handler : ChangeHandler<PingWithFailureInCommandHandler>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<ChangeResult> Handle(PingWithFailureInCommandHandler command, CancellationToken cancellationToken)
        {
            await Publish(new Pong("Command received", command.Id, Guid.NewGuid()), cancellationToken);
            throw new PingError().ToException();
        }
    }
}