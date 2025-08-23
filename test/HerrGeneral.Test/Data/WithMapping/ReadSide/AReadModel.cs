using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithMapping.ReadSide;

public class AReadModel : EventTracker, ILocalEventHandler<Pong>, ILocalEventHandler<PongPong>, ILocalEventHandler<PongPongPong>
{
    public readonly Guid Id = Guid.NewGuid();
    
    public void Handle(Pong notification) =>
        AddHandled(notification);

    public void Handle(PongPong notification) =>
        AddHandled(notification);

    public void Handle(PongPongPong notification) =>
        AddHandled(notification);
}