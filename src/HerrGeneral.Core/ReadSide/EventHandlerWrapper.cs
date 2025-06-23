using HerrGeneral.Core.WriteSide;
using HerrGeneral.ReadSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.ReadSide;

internal class EventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    private static void Handle(UnitOfWorkId operationId, TEvent @event, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<IEventHandler<TEvent>>>();
        var stringBuilderLogger = serviceProvider.GetRequiredService<CommandLogger>().GetStringBuilder(operationId);

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            Start(handler)
                .WithReadSideHandlerLogging(logger, handler, stringBuilderLogger)
                (operationId, @event);
        }
    }

    public void Handle(UnitOfWorkId operationId, object @event, IServiceProvider serviceProvider) =>
        Handle(operationId, (TEvent)@event, serviceProvider);
    
    private static ReadSidePipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> handler) =>
        (_, @event) =>  handler.Handle(@event);
}