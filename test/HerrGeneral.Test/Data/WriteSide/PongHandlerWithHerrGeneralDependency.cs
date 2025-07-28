namespace HerrGeneral.Test.Data.WriteSide;

public class PongHandlerWithHerrGeneralDependency(Dependency2 dependency) : HerrGeneral.WriteSide.IEventHandler<Pong>
{
    public void Handle(Pong notification) => dependency.Called = true;
}