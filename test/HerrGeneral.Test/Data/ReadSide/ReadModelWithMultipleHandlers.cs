using HerrGeneral.ReadSide;
using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class ReadModelWithMultipleHandlers
{
    public string Message { get; private set; } = string.Empty;

    public class Repository(ReadModelWithMultipleHandlers readModel) :
        IEventHandler<Pong>,
        IEventHandler<AnotherPong>
    {
        public void Handle(Pong notification) => 
            readModel.Message = notification.Message;

        public void Handle(AnotherPong notification) => 
            readModel.Message = notification.Message;
    }
}