using System.Collections.Concurrent;
using System.Linq.Expressions;
using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Core.ReadSide;

internal sealed class PostTransactionEventDispatcher(
    IServiceProvider serviceProvider,
    CommandExecutionTracer? commandExecutionTracer = null)
{
    private static readonly ConcurrentDictionary<Type, IPostTransactionEventHandlerWrapper> EventHandlerWrappers = new();

    public void Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        commandExecutionTracer?.StartPublishEventsOnPostTransaction(events.Count);
        foreach (var eventToDispatch in events)
        {
            commandExecutionTracer?.PublishEventOnPostTransaction(eventToDispatch);
            DispatchSingle(eventToDispatch);
        }
    }

    public void Dispatch(params object[] events) => Dispatch((IReadOnlyList<object>)events);

    private void DispatchSingle(object eventToDispatch)
    {
        var wrapper = EventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), CreateWrapper);
        wrapper.Handle(eventToDispatch, serviceProvider);
    }

    private static IPostTransactionEventHandlerWrapper CreateWrapper(Type eventType)
    {
        var wrapperType = typeof(PostTransactionEventHandlerWrapper<>).MakeGenericType(eventType);
        var newExpr = Expression.New(wrapperType);
        var lambda = Expression.Lambda<Func<IPostTransactionEventHandlerWrapper>>(newExpr);
        return lambda.Compile()();
    }
}
