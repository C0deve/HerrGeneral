using HerrGeneral.Core.WriteSide;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.ReadSide;

internal static class ReadSidePipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(TEvent @event);

    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(
        this EventHandlerDelegate<TEvent> next,
        ILogger<IEventHandler<TEvent>>? logger,
        IEventHandler<TEvent> handler,
        CommandLogger stringBuilderLogger) =>
        @event =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if (logger.IsEnabled(LogLevel.Debug))
                stringBuilderLogger.HandleEvent(handler.GetType());

            next(@event);
        };
}