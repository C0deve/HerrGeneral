using HerrGeneral.Contracts;
using HerrGeneral.Error;
using HerrGeneral.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.ReadSide.Dispatcher;

internal static class Pipeline
{
    public delegate Task EventHandlerDelegate<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
    
    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<Contracts.IEventHandler<TEvent>>? logger, Contracts.IEventHandler<TEvent> handler) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<Contracts.IEventHandler<TEvent>>.Instance;

            try
            {
                logger.LogReadSideEventHandler(handler.GetType());
                await next(@event, cancellationToken);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw;
            }
        };
    
    public static EventHandlerDelegate<TEvent> WithReadSidException<TEvent>(this EventHandlerDelegate<TEvent> next) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            try
            {
                await next(@event, cancellationToken);
            }
            catch (Exception e)
            {
                throw new ReadSideException(e);
            }
        };
}