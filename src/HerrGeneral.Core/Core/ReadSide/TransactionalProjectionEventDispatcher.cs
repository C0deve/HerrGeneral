using System.Collections.Concurrent;
using System.Linq.Expressions;
using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Core.ReadSide;

internal sealed class TransactionalProjectionEventDispatcher(
    IServiceProvider serviceProvider,
    CommandExecutionTracer? commandExecutionTracer = null)
{
    private static readonly ConcurrentDictionary<Type, ISyncProjectionEventHandlerWrapper> EventHandlerWrappers = new();

    public void Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        commandExecutionTracer?.StartPublishEventsOnSyncProjections(events.Count);
        foreach (var eventToDispatch in events)
        {
            commandExecutionTracer?.PublishEventOnSyncProjections(eventToDispatch);
            DispatchSingle(eventToDispatch);
        }
    }

    public void Dispatch(params object[] events) => Dispatch((IReadOnlyList<object>)events);

    private void DispatchSingle(object eventToDispatch)
    {
        var wrapper = EventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), CreateWrapper);
        wrapper.Handle(eventToDispatch, serviceProvider);
    }

    private static ISyncProjectionEventHandlerWrapper CreateWrapper(Type eventType)
    {
        var wrapperType = typeof(SyncProjectionEventHandlerWrapper<>).MakeGenericType(eventType);
        var newExpr = Expression.New(wrapperType);
        var lambda = Expression.Lambda<Func<ISyncProjectionEventHandlerWrapper>>(newExpr);
        return lambda.Compile()();
    }
}
