using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongWithFailureInEventHandlerHandler : IEventHandler<PongWithFailureInEventHandlerEvent>
{
    public void Handle(PongWithFailureInEventHandlerEvent notification, CancellationToken cancellationToken) => 
        throw new PingError();
}