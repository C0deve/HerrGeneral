using HerrGeneral.Core.ReadSide;
using HerrGeneral.Exception;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal static class EventHandlerPipeline
{
    public delegate IEnumerable<object> EventHandlerDelegate<in TEvent>(TEvent @event);

    extension<TEvent>(EventHandlerDelegate<TEvent> next)
    {
        public EventHandlerDelegate<TEvent> WithDomainExceptionMapping(DomainExceptionMapper mapper) =>
            @event =>
            {
                try
                {
                    return next(@event);
                }
                catch (System.Exception e)
                {
                    throw mapper.Map(e,
                        exception => new EventHandlerDomainException(exception),
                        exception => new EventHandlerException(exception));
                }
            };

        public EventHandlerDelegate<TEvent> WithTracer(IEventHandler<TEvent> handler, CommandExecutionTracer? tracer) =>
            @event =>
            {
                if (tracer is null)
                {
                    return next(@event);
                }

                try
                {
                    if (handler is IHandlerTypeProvider handlerTypeProvider)
                        tracer.HandleEvent(handlerTypeProvider.GetHandlerType());
                    else
                        tracer.HandleEvent(handler.GetType());
                    return next(@event);
                }
                catch (EventHandlerDomainException e)
                {
                    tracer.OnException(e, 2);
                    throw;
                }
                catch (EventHandlerException e)
                {
                    tracer.OnException(e.InnerException!, 2);
                    throw;
                }
            };
    }
}