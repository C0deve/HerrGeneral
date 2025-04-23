using HerrGeneral.Core;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.ReadSideEventDispatcher.Test
{
    public class ReadSideEventDispatcherShould(ITestOutputHelper output)
    {
        [Fact]
        public void Dispatch_event()
        {
            var container = new Container(cfg =>
            {
                cfg.AddHerrGeneralTestLogger(output);

                cfg.ForSingletonOf<ReadModel>().Use<ReadModel>();

                cfg.UseHerrGeneral(scanner =>
                    scanner
                        .AddWriteSideAssembly(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!)
                        .AddReadSideAssembly(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!));
            });
            var operationId = UnitOfWorkId.New();

            container.GetInstance<IAddEventToDispatch>().AddEventToDispatch(operationId, new Pong(
                "Pong received",
                Guid.NewGuid(),
                Guid.NewGuid()));
            container.GetInstance<Core.ReadSide.ReadSideEventDispatcher>().Dispatch(operationId, CancellationToken.None);

            container.GetInstance<ReadModel>().Message.ShouldBe("Pong received");
        }

        private class ReadModel
        {
            public string Message { get; set; } = string.Empty;
        }

        private record Pong : EventBase
        {
            public string Message { get; }

            public Pong(string message, Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) =>
                Message = message;
        }

        private record PongHandler : ReadSide.IEventHandler<Pong>
        {
            private readonly ReadModel _readModel;

            public PongHandler(ReadModel readModel) => _readModel = readModel;

            public void Handle(Pong notification, CancellationToken cancellationToken) => 
                _readModel.Message = notification.Message;
        }
    }
}