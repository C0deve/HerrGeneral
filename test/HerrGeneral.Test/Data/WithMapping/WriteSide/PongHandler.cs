namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class PongHandler(EventTracker eventTracker) : ILocalEventHandler<Pong>
{
    public MyEventHandlerResult Handle(Pong notification)
    {
        var pongPong = new PongPong(notification.SourceCommandId, Guid.NewGuid());
        eventTracker.AddHandled(pongPong);
        return new MyEventHandlerResult(pongPong);
    }
}