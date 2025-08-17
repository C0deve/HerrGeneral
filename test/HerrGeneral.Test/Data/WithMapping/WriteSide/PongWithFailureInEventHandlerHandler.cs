namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class PongWithFailureInEventHandlerHandler : ILocalEventHandler<PongWithFailureInEventHandlerEvent>
{
    public MyEventHandlerResult Handle(PongWithFailureInEventHandlerEvent notification) => 
        throw new PingError();
}