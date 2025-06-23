namespace HerrGeneral.Test.Data.WriteSide;

public class PongWithFailureInEventHandlerHandler : ILocalEventHandler<PongWithFailureInEventHandlerEvent>
{
    public void Handle(PongWithFailureInEventHandlerEvent notification) => 
        throw new PingError();
}