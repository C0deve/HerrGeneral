using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;

public class ReadModelWithMultipleHandlersAndInheritingIEventHandler :
    EventTracker,
    HerrGeneral.ReadSide.IEventHandler<Pong>,
    HerrGeneral.ReadSide.IEventHandler<PongPong>,
    HerrGeneral.ReadSide.IEventHandler<AnotherPong>
{
    public void Handle(Pong notification) =>
        AddHandled(notification);

    public void Handle(AnotherPong notification) =>
        AddHandled(notification);

    public void Handle(PongPong notification)=>
        AddHandled(notification);
}