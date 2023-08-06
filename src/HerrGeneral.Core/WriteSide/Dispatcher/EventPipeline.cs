using HerrGeneral.Contracts.WriteSide;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide.Dispatcher;

internal static class EventPipeline
{
    public delegate Task EventHandlerDelegate<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    public static EventHandlerDelegate<TEvent> WithHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            try
            {
                logger.StartHandling(handler, @event);
                await next(@event, cancellationToken);
            }
            finally
            {
                logger.StopHandling(handler);
            }
        };
    
    public static EventHandlerDelegate<TEvent> WithErrorLogger<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            try
            {
                await next(@event, cancellationToken);
            }
            catch (DomainException e)
            {
                logger.Log(e, 2);
                throw;
            }
            catch (Exception e)
            {
                logger.Log(e, 2);
                throw;
            }
        };
    
    public static EventHandlerDelegate<TEvent> WithErrorMapping<TEvent>(this EventHandlerDelegate<TEvent> next) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            try
            {
                await next(@event, cancellationToken);
            }
            catch (DomainException e)
            {
                throw new EventHandlerDomainException(e);
            }
            catch (Exception e)
            {
                throw new EventHandlerException(e);
            }
        };
}