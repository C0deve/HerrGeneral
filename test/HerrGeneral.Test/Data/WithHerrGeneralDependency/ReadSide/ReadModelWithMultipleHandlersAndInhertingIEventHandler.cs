using HerrGeneral.ReadSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;

public class ProjectionWithMultipleHandlersAndInheritingIProjectionEventHandler :
    EventTracker,
    IProjectionEventHandler<Pong>,
    IProjectionEventHandler<PongPong>,
    IProjectionEventHandler<AnotherPong>
{
    public void Handle(Pong notification) =>
        AddHandled(notification);

    public void Handle(AnotherPong notification) =>
        AddHandled(notification);

    public void Handle(PongPong notification)=>
        AddHandled(notification);
}