using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var tracer = serviceProvider.GetService<CommandExecutionTracer>();
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithTracer(handler, tracer)
                (@event);
        }
    }
    
    private static EventHandlerPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        eventHandler.Handle;
}