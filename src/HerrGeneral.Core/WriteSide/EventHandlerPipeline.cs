using System.Text;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal static class EventHandlerPipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(UnitOfWorkId operationId, TEvent @event, CancellationToken cancellationToken);

    public static EventHandlerDelegate<TEvent> WithDomainExceptionMapping<TEvent>(
        this EventHandlerDelegate<TEvent> next, DomainExceptionMapper mapper) =>
        (operationId, @event, cancellationToken) =>
        {
            try
            {
                next(operationId, @event, cancellationToken);
            }
            catch (Exception e)
            {
                throw mapper.Map(e,
                    exception => new EventHandlerDomainException(exception),
                    exception => new EventHandlerException(exception));
            }
        };

    public static EventHandlerDelegate<TEvent> WithErrorLogger<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, StringBuilder stringBuilderLogger) =>
        (operationId, @event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if (!logger.IsEnabled(LogLevel.Debug))
                next(operationId, @event, cancellationToken);

            try
            {
                next(operationId, @event, cancellationToken);
            }
            catch (EventHandlerDomainException e)
            {
                stringBuilderLogger.OnException(e, 2);
                throw;
            }
            catch (EventHandlerException e)
            {
                stringBuilderLogger.OnException(e.InnerException!, 2);
                throw;
            }
        };

    public static EventHandlerDelegate<TEvent> WithLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler, StringBuilder stringBuilderLogger) =>
        (operationId, @event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if (logger.IsEnabled(LogLevel.Debug))
                stringBuilderLogger.HandleEvent(handler.GetType());

            next(operationId, @event, cancellationToken);
        };
}