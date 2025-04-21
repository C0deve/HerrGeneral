using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongInternalHandler(Dependency dependency) : IEventHandler<Pong>
{
    public void Handle(Pong notification, CancellationToken cancellationToken) => 
        dependency.Called = true;
}