using System.Text;
using HerrGeneral.Contracts;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal static class WriteSideEventPipeline
{
    public delegate Task EventHandlerDelegate<in TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;

    public static EventHandlerDelegate<TEvent> WithHandlerLogging<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, IEventHandler<TEvent> handler, StringBuilder stringBuilderLogger) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if(logger.IsEnabled(LogLevel.Debug))
                stringBuilderLogger.HandleEvent(handler.GetType());
            
            await next(@event, cancellationToken);
        };
    
    public static EventHandlerDelegate<TEvent> WithErrorLogger<TEvent>(this EventHandlerDelegate<TEvent> next, ILogger<IEventHandler<TEvent>>? logger, StringBuilder stringBuilderLogger) where TEvent : IEvent =>
        async (@event, cancellationToken) =>
        {
            logger ??= NullLogger<IEventHandler<TEvent>>.Instance;
            if(!logger.IsEnabled(LogLevel.Debug))
                await next(@event, cancellationToken);
            
            try
            {
                await next(@event, cancellationToken);
            }
            catch (DomainException e)
            {
                stringBuilderLogger.OnException(e, 2);
                throw;
            }
            catch (Exception e)
            {
                stringBuilderLogger.OnException(e, 2);
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