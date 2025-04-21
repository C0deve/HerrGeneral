using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongMiddleHandler(Dependency dependency) : IEventHandler<Pong>
{
    public void Handle(Pong notification, CancellationToken cancellationToken)
    {
        dependency.Called = true;
    }
}