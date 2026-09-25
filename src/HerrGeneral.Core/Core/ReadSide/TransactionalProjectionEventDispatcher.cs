using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.ReadSide;

internal sealed class TransactionalProjectionEventDispatcher(IServiceProvider serviceProvider)
{
    private static readonly ConcurrentDictionary<Type, ISyncProjectionEventHandlerWrapper> EventHandlerWrappers = new();

    public void Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        using var activity = HerrGeneralDiagnostics.StartActivity(
            HerrGeneralDiagnostics.Activities.SyncProjectionsDispatch);

        activity?.SetTag(HerrGeneralDiagnostics.Tags.EventsCount, events.Count);
        activity?.SetTag(HerrGeneralDiagnostics.Tags.ThreadId, Environment.CurrentManagedThreadId);

        HerrGeneralDiagnostics.EventsTotal.Add(events.Count,
            new KeyValuePair<string, object?>("herrgeneral.stage", "SyncProjections"));

        foreach (var eventToDispatch in events)
        {
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
