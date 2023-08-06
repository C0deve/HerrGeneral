using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongWithFailureInEventHandlerHandler : IEventHandler<PongWithFailureInEventHandlerEvent>
{
    public Task Handle(PongWithFailureInEventHandlerEvent notification, CancellationToken cancellationToken) => 
        throw new PingError().ToException();
}