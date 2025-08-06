using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var tracer = serviceProvider.GetRequiredService<CommandExecutionTracer>();
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithErrorLogger(logger, tracer)
                .WithLogging(logger, handler, tracer)
                (@event);
        }
    }
    
    private static EventHandlerPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        eventHandler.Handle;
}