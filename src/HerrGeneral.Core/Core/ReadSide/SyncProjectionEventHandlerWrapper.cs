using System.Linq.Expressions;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.ReadSide;

internal interface ISyncProjectionEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}

internal class SyncProjectionEventHandlerWrapper<TEvent> : ISyncProjectionEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var tracer = serviceProvider.GetService<CommandExecutionTracer>();

        foreach (var handler in serviceProvider.GetServices<IHandleSyncProjection<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            tracer?.HandleSyncProjection(handlerType);

            handler.Handle(@event);
        }
    }
}
