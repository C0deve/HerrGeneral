using HerrGeneral.ReadSide;
using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class AReadModelWithFailure : IEventHandler<PongWithReadSideFailure>
{
    public Task Handle(PongWithReadSideFailure notification, CancellationToken cancellationToken)
    {
        throw new SomePanicException();
    }
}