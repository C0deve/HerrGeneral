using System.Text;
using HerrGeneral.Core.Logger;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.ReadSide;

internal static class ReadSidePipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(Guid operationId, TEvent @event, CancellationToken cancellationToken);

    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler, StringBuilder stringBuilderLogger) =>
        (operationId, @event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if (logger.IsEnabled(LogLevel.Debug))
                stringBuilderLogger.HandleEvent(handler.GetType());

            next(operationId, @event, cancellationToken);
        };
}