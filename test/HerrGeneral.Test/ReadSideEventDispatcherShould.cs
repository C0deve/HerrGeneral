using HerrGeneral.Error;
using HerrGeneral.ReadSide.Dispatcher;
using HerrGeneral.Registration;
using HerrGeneral.Test.Extension.Log;
using HerrGeneral.WriteSide;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.ReadSideEventDispatcher.Test
{
    public class ReadSideEventDispatcherShould
    {
        private readonly ITestOutputHelper _output;

        public ReadSideEventDispatcherShould(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Dispatch_event()
        {
            var container = new Container(cfg =>
            {
                cfg.AddHerrGeneralTestLogger(_output);

                cfg.ForSingletonOf<ReadModel>().Use<ReadModel>();

                cfg.UseHerrGeneral(scanner =>
                    scanner
                        .OnWriteSide(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!)
                        .OnReadSide(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!));
            });
            var sourceCommandId = Guid.NewGuid();

            container.GetInstance<IAddEventToDispatch>().AddEventToDispatch(
                new Pong(
                    "Pong received",
                    sourceCommandId,
                    Guid.NewGuid()));
            await container.GetInstance<HerrGeneral.ReadSide.IEventDispatcher>().Dispatch(sourceCommandId, CancellationToken.None);

            container.GetInstance<ReadModel>().Message.ShouldBe("Pong received");
        }

        [Fact]
        public async Task Throw_ReadSideException()
        {
            var container = new Container(cfg =>
            {
                cfg.AddHerrGeneralTestLogger(_output);
                
                cfg.ForSingletonOf<ReadModel>().Use<ReadModel>();

                cfg.UseHerrGeneral(scanner =>
                    scanner
                        .OnWriteSide(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!)
                        .OnReadSide(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!));
            });
            var sourceCommandId = Guid.NewGuid();

            container.GetInstance<IAddEventToDispatch>()
                .AddEventToDispatch(
                    new PongWithFailure(
                        sourceCommandId,
                        Guid.NewGuid()));

            async Task Act() =>
                await container.GetInstance<HerrGeneral.ReadSide.IEventDispatcher>()
                    .Dispatch(sourceCommandId, CancellationToken.None);

            await Should.ThrowAsync<ReadSideException>(Act);
        }

        private class ReadModel
        {
            public string Message { get; set; } = string.Empty;
        }

        private class Pong : EventBase
        {
            public string Message { get; }

            public Pong(string message, Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) =>
                Message = message;
        }

        private class PongHandler : ReadSide.Contracts.IEventHandler<Pong>
        {
            private readonly ReadModel _readModel;

            public PongHandler(ReadModel readModel) => _readModel = readModel;

            public Task Handle(Pong notification, CancellationToken cancellationToken)
            {
                _readModel.Message = notification.Message;
                return Task.CompletedTask;
            }
        }

        private class PongWithFailure : EventBase
        {
            public PongWithFailure(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
            {
            }
        }

        private class PongWithFailureHandler : ReadSide.Contracts.IEventHandler<PongWithFailure>
        {
            public Task Handle(PongWithFailure notification, CancellationToken cancellationToken) =>
                throw new Exception("Exception from ReadSide handler");
        }
    }
}