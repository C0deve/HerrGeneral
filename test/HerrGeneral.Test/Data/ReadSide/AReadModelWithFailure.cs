using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class AReadModelWithFailure : ILocalEventHandler<PongWithReadSideFailure>
{
    public void Handle(PongWithReadSideFailure notification) => 
        throw new SomePanicException();
}