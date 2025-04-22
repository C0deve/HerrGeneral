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
            var sourceCommandId = Guid.NewGuid();

            container.GetInstance<IAddEventToDispatch>().AddEventToDispatch(sourceCommandId, new Pong(
                "Pong received",
                sourceCommandId,
                Guid.NewGuid()));
            container.GetInstance<IEventDispatcher>().Dispatch(sourceCommandId, CancellationToken.None);

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

        private record PongWithFailure : EventBase
        {
            public PongWithFailure(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
            {
            }
        }

        private class PongWithFailureHandler : ReadSide.IEventHandler<PongWithFailure>
        {
            public void Handle(PongWithFailure notification, CancellationToken cancellationToken) =>
                throw new Exception("Exception from ReadSide handler");
        }
    }
}