namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class PongHandler(CommandTracker2 commandTracker) : ILocalEventHandler<Pong>
{
    public void Handle(Pong notification) => 
        commandTracker.AddHandled(notification.SourceCommandId);
}