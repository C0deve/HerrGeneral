using HerrGeneral.Contracts;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.ReadSide;

internal static class Pipeline
{
    public delegate Task EventHandlerDelegate<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
    
    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;

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