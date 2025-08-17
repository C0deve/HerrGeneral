using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

public class PongHandlerWithHerrGeneralDependency(CommandTracker3 commandTracker) : HerrGeneral.WriteSide.IEventHandler<Pong>
{
    public IEnumerable<object> Handle(Pong notification)
    {
         commandTracker.AddHandled(notification.SourceCommandId);
         return [];
    }
}