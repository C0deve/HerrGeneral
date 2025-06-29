using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class ReadModel
{
    public string Message { get; private set; } = string.Empty;

    public class Repository(ReadModel readModel) : ILocalEventHandler<Pong>
    {
        public readonly Guid Id = Guid.NewGuid();

        public void Handle(Pong notification) => 
            readModel.Message = notification.Message;
    }
}