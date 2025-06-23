using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    private static void Handle(UnitOfWorkId operationId, TEvent @event, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var stringBuilderLogger = serviceProvider.GetRequiredService<CommandLogger>().GetStringBuilder(operationId);
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithErrorLogger(logger, stringBuilderLogger)
                .WithLogging(logger, handler, stringBuilderLogger)
                (operationId, @event);
        }
    }

    public void Handle(UnitOfWorkId operationId, object @event, IServiceProvider serviceProvider) =>
        Handle(operationId, (TEvent)@event, serviceProvider);

    private static EventHandlerPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        (_, @event) => eventHandler.Handle(@event);
}