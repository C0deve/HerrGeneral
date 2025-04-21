using HerrGeneral.ReadSide;
using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class ReadModel
{
    public string Message { get; private set; } = string.Empty;

    public class Repository(ReadModel readModel) : IEventHandler<Pong>
    {
        public readonly Guid Id = Guid.NewGuid();

        public void Handle(Pong notification, CancellationToken cancellationToken) => 
            readModel.Message = notification.Message;
    }
}