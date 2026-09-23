using HerrGeneral.Core.Diagnostics;
using HerrGeneral.ReadSide;

namespace HerrGeneral.Core.ReadSide;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var collector = serviceProvider.GetService<ActivityTreeCollector>();

        foreach (var handler in serviceProvider.GetServices<IProjectionEventHandler<TEvent>>())
        {
            Start(handler)
                .WithReadSideHandlerLogging(handler, collector)
                (@event);
        }
    }
    
    private static ReadSidePipeline.EventHandlerDelegate<TEvent> Start(IProjectionEventHandler<TEvent> handler) =>
        handler.Handle;
}