using HerrGeneral.Core.WriteSide;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.ReadSide;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var tracer = serviceProvider.GetRequiredService<CommandExecutionTracer>();

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithReadSideHandlerLogging(logger, handler, tracer)
                (@event);
        }
    }
    
    private static ReadSidePipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> handler) =>
        handler.Handle;
}