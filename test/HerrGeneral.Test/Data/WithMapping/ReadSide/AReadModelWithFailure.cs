using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithMapping.ReadSide;

public class AReadModelWithFailure : ILocalEventHandler<PongWithReadSideFailure>
{
    public void Handle(PongWithReadSideFailure notification) => 
        throw new SomePanicException();
}