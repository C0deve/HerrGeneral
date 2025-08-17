namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class PongHandler(CommandTracker2 commandTracker, EventTracker eventTracker) : ILocalEventHandler<Pong>
{
    public MyEventHandlerResult Handle(Pong notification)
    {
        commandTracker.AddHandled(notification.SourceCommandId);

        var pongPong = new PongPong(notification.SourceCommandId, Guid.NewGuid());
        eventTracker.AddHandled(pongPong);
        return new MyEventHandlerResult(pongPong);
    }
}