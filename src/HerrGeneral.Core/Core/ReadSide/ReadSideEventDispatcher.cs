

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal sealed class ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandExecutionTracer? commandExecutionTracer = null)
{
    private static readonly ConcurrentDictionary<Type, IEventHandlerWrapper> EventHandlerWrappers = new();

    public void Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        commandExecutionTracer?.StartPublishEventsOnReadSide(events.Count);
        foreach (var eventToDispatch in events)
        {
            commandExecutionTracer?.PublishEventOnReadSide(eventToDispatch);
            DispatchSingle(eventToDispatch);
        }
    }

    public void Dispatch(params object[] events) => Dispatch((IReadOnlyList<object>)events);

    /// <summary>
    /// Dispatch the event using an instance of EventHandlerWrapper
    /// </summary>
    /// <param name="eventToDispatch"></param>
    private void DispatchSingle(object eventToDispatch)
    {
        var wrapper = EventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), CreateWrapper);
        wrapper.Handle(eventToDispatch, serviceProvider);
    }

    private static IEventHandlerWrapper CreateWrapper(Type eventType)
    {
        var wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
        var newExpr = Expression.New(wrapperType);
        var lambda = Expression.Lambda<Func<IEventHandlerWrapper>>(newExpr);
        return lambda.Compile()();
    }
}
