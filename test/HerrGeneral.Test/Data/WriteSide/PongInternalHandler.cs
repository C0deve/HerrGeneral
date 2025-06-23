namespace HerrGeneral.Test.Data.WriteSide;

public class PongInternalHandler(Dependency dependency) : ILocalEventHandler<Pong>
{
    public void Handle(Pong notification) => 
        dependency.Called = true;
}