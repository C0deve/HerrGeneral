namespace HerrGeneral.Test.Data.WriteSide;

public class PongMiddleHandler(Dependency dependency) : ILocalEventHandler<Pong>
{
    public void Handle(Pong notification)
    {
        dependency.Called = true;
    }
}