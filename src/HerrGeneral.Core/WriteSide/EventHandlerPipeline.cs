using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal static class EventHandlerPipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(TEvent @event);

    public static EventHandlerDelegate<TEvent> WithDomainExceptionMapping<TEvent>(
        this EventHandlerDelegate<TEvent> next, DomainExceptionMapper mapper) =>
        @event =>
        {
            try
            {
                next(@event);
            }
            catch (Exception e)
            {
                throw mapper.Map(e,
                    exception => new EventHandlerDomainException(exception),
                    exception => new EventHandlerException(exception));
            }
        };

    public static EventHandlerDelegate<TEvent> WithTracer<TEvent>(this EventHandlerDelegate<TEvent> next, IEventHandler<TEvent> handler, CommandExecutionTracer? tracer) =>
        @event =>
        {
            if (tracer is null)
            {
                next(@event);
                return;
            }

            try
            {
                tracer.HandleEvent(handler.GetType());
                next(@event);
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