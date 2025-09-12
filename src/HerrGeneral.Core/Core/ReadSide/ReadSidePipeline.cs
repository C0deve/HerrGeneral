using HerrGeneral.Core.WriteSide;
using HerrGeneral.ReadSide;

namespace HerrGeneral.Core.ReadSide;

internal static class ReadSidePipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(TEvent @event);

    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(
        this EventHandlerDelegate<TEvent> next,
        IProjectionEventHandler<TEvent> handler,
        CommandExecutionTracer? tracer) =>
        @event =>
        {
            if (handler is IHandlerTypeProvider handlerTypeProvider)
                tracer?.HandleEvent(handlerTypeProvider.GetHandlerType());
            else
                tracer?.HandleEvent(handler.GetType());
            next(@event);
        };
}