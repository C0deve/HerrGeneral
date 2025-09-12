using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using Microsoft.Extensions.DependencyInjection;
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
            var services = new ServiceCollection()
                .AddSingleton<PongHandler>()
                .AddHerrGeneralTestLogger(output)
                .AddHerrGeneral(scanner =>
                    scanner
                        .ScanWriteSideOn(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!)
                        .ScanReadSideOn(typeof(PongHandler).Assembly, typeof(PongHandler).Namespace!));

            services.AddSingleton<ReadModel, ReadModel>();

            var container = services.BuildServiceProvider();
            container.GetRequiredService<Core.ReadSide.ReadSideEventDispatcher>().Dispatch(new Pong(
                "Pong received",
                Guid.NewGuid(),
                Guid.NewGuid()));

            container.GetRequiredService<ReadModel>().Message.ShouldBe("Pong received");
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

        private record PongHandler : ReadSide.IProjectionEventHandler<Pong>
        {
            private readonly ReadModel _readModel;

            public PongHandler(ReadModel readModel) => _readModel = readModel;

            public void Handle(Pong notification) =>
                _readModel.Message = notification.Message;
        }
    }
}