using HerrGeneral.Contracts;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
    where TEvent : IEvent
{
    private static async Task Handle(TEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var stringBuilderLogger = serviceProvider.GetRequiredService<CommandLogger>().GetStringBuilder(@event.SourceCommandId);
        
        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            await
                Start(handler)
                    .WithErrorLogger(logger, stringBuilderLogger)
                    .WithErrorMapping()
                    .WithHandlerLogging(logger, handler, stringBuilderLogger)
                    (@event, cancellationToken);
        }
    }

    public async Task Handle(object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await Handle((TEvent)@event, serviceProvider, cancellationToken);

    private static WriteSideEventPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        eventHandler.Handle;
}