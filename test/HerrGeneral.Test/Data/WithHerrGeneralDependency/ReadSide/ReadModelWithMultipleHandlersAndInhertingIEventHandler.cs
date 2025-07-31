using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;

public class ReadModelWithMultipleHandlersAndInheritingIEventHandler :
    CommandTracker,
    HerrGeneral.ReadSide.IEventHandler<Pong>,
    HerrGeneral.ReadSide.IEventHandler<AnotherPong>
{
    public void Handle(Pong notification) =>
        AddHandled(notification.SourceCommandId);

    public void Handle(AnotherPong notification) =>
        AddHandled(notification.SourceCommandId);
}