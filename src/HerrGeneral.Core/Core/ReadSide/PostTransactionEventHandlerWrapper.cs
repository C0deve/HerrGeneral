using System.Linq.Expressions;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.ReadSide;

internal interface IPostTransactionEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}

internal class PostTransactionEventHandlerWrapper<TEvent> : IPostTransactionEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var tracer = serviceProvider.GetService<CommandExecutionTracer>();

        // 1. Post-transaction Projections (IHandlePostProjection and IProjectionEventHandler)
        foreach (var handler in serviceProvider.GetServices<IHandlePostProjection<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            try
            {
                tracer?.HandlePostProjectionEvent(handlerType);
                handler.Handle(@event);
            }
            catch (System.Exception ex)
            {
                tracer?.OnPostTransactionException(ex, handlerType);
            }
        }

        // 2. Post-transaction Side Effects (IHandleSideEffect)
        foreach (var handler in serviceProvider.GetServices<IHandleSideEffect<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            try
            {
                tracer?.HandleSideEffectEvent(handlerType);
                handler.Handle(@event);
            }
            catch (System.Exception ex)
            {
                tracer?.OnPostTransactionException(ex, handlerType);
            }
        }
    }
}
