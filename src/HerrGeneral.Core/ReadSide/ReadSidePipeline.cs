using System.Text;
using HerrGeneral.Contracts;
using HerrGeneral.Core.Logger;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.ReadSide;

internal static class ReadSidePipeline
{
    public delegate Task EventHandlerDelegate<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler, StringBuilder stringBuilderLogger) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if (logger.IsEnabled(LogLevel.Debug))
                stringBuilderLogger.HandleEvent(handler.GetType());
            
            await next(@event, cancellationToken);
        };
}