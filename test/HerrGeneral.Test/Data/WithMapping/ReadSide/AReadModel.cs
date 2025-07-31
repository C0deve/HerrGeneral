using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithMapping.ReadSide;

public class AReadModel : CommandTracker, ILocalEventHandler<Pong>
{
    public readonly Guid Id = Guid.NewGuid();
    
    public void Handle(Pong notification) =>
        AddHandled(notification.SourceCommandId);
}