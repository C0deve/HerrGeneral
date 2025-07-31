using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithMapping.ReadSide;

public class AReadModelWithMultipleHandlers :
    ILocalEventHandler<Pong>,
    ILocalEventHandler<AnotherPong>
{
    public void Handle(Pong notification)
    {
    }

    public void Handle(AnotherPong notification)
    {
    }
}