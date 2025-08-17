namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class PongPongHandler(EventTracker eventTracker) : ILocalEventHandler<PongPong>
{
    public MyEventHandlerResult Handle(PongPong notification)
    {
        var pongPongPong = new PongPongPong(notification.SourceCommandId, Guid.NewGuid());
        eventTracker.AddHandled(pongPongPong);
        return new MyEventHandlerResult(pongPongPong);
    }
}