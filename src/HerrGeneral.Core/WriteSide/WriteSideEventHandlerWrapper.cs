using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    private static void Handle(UnitOfWorkId operationId, TEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var stringBuilderLogger = serviceProvider.GetRequiredService<CommandLogger>().GetStringBuilder(operationId);

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithErrorLogger(logger, stringBuilderLogger)
                .WithErrorMapping()
                .WithHandlerLogging(logger, handler, stringBuilderLogger)
                (operationId, @event, cancellationToken);
        }
    }

    public void Handle(UnitOfWorkId operationId, object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        Handle(operationId, (TEvent)@event, serviceProvider, cancellationToken);

    private static WriteSideEventPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        (_, @event, token) => eventHandler.Handle(@event, token);
}