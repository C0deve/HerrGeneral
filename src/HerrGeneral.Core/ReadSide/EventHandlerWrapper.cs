using HerrGeneral.Core.WriteSide;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.ReadSide;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    private static void Handle(Guid operationId, TEvent @event, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var stringBuilderLogger = serviceProvider.GetRequiredService<CommandLogger>().GetStringBuilder(operationId);

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithReadSideHandlerLogging(logger, handler, stringBuilderLogger)
                (operationId, @event, cancellationToken);
        }
    }

    public void Handle(Guid operationId, object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        Handle(operationId, (TEvent)@event, serviceProvider, cancellationToken);
    
    private static ReadSidePipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> handler) =>
        (_, @event, token) =>  handler.Handle(@event, token);
}