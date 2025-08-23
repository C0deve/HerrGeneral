using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

public class PongHandlerWithHerrGeneralDependency(EventTracker eventTracker) : HerrGeneral.WriteSide.IEventHandler<Pong>
{
    public IEnumerable<object> Handle(Pong notification)
    {
        var pongPong = new PongPong(notification.SourceCommandId, Guid.NewGuid());
         eventTracker.AddHandled(pongPong);
         return [];
    }
}