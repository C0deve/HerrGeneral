using HerrGeneral.ReadSide;
using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class ReadModelWithMultipleHandlers
{
    public string Message { get; private set; } = string.Empty;

    public class Repository : 
        IEventHandler<Pong>,
        IEventHandler<AnotherPong>
    {
        private readonly ReadModelWithMultipleHandlers _readModel;

        public Repository(ReadModelWithMultipleHandlers readModel)
        {
            _readModel = readModel;
        }

        public Task Handle(Pong notification, CancellationToken cancellationToken)
        {
            _readModel.Message = notification.Message;
            return Task.CompletedTask;
        }

        public Task Handle(AnotherPong notification, CancellationToken cancellationToken)
        {
            _readModel.Message = notification.Message;
            return Task.CompletedTask;
        }
    }
}