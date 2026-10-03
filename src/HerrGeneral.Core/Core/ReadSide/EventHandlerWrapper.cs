using HerrGeneral.Core.Diagnostics;
using HerrGeneral.ReadSide;
using static HerrGeneral.Core.ReadSide.ReadSidePipeline;

namespace HerrGeneral.Core.ReadSide;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var handlers = serviceProvider.GetServices<IProjectionEventHandler<TEvent>>();
        if (handlers is ICollection<IProjectionEventHandler<TEvent>> { Count: 0 })
        {
            return;
        }

        var collector = serviceProvider.GetService<ActivityTreeCollector>();

        // Dispatch the read-side event to all registered projection handlers through the pipeline (tracing and diagnostics)
        foreach (var handler in handlers)
        {
            EventHandlerDelegate<TEvent> pipeline = handler.Handle;
            pipeline.WithTracer(handler, collector)(@event);
        }
    }
}