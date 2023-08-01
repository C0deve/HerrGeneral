using HerrGeneral.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.ReadSide.Dispatcher;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
    where TEvent : IEvent
{
    private static async Task Handle(TEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetService<ILogger<HerrGeneral.ReadSide.Contracts.IEventHandler<TEvent>>>();

        foreach (var handler in serviceProvider.GetServices<HerrGeneral.ReadSide.Contracts.IEventHandler<TEvent>>())
        {
            await 
                Start(handler)
                    .WithReadSidException()
                    .WithReadSideHandlerLogging(logger, handler)
                    (@event, cancellationToken);
        }
    }

    public async Task Handle(object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await Handle((TEvent)@event, serviceProvider, cancellationToken);
    
    private static Pipeline.EventHandlerDelegate<TEvent> Start(HerrGeneral.ReadSide.Contracts.IEventHandler<TEvent> handler) =>
        handler.Handle;
}