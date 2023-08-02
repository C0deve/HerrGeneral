using HerrGeneral.Contracts.ReadSIde;
using HerrGeneral.Test.Data.WriteSide;

namespace HerrGeneral.Test.Data.ReadSide;

public class ReadModel
{
    public string Message { get; private set; } = string.Empty;

    public class Repository : IEventHandler<Pong>
    {
        private readonly ReadModel _readModel;

        public readonly Guid Id;

        public Repository(ReadModel readModel)
        {
            _readModel = readModel;
            Id = Guid.NewGuid();
        }

        public Task Handle(Pong notification, CancellationToken cancellationToken)
        {
            _readModel.Message = notification.Message;
            return Task.CompletedTask;
        }
    }
}