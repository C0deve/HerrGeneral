using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.ReadSide;

internal sealed class PostTransactionEventDispatcher(IServiceProvider serviceProvider)
{
    private static readonly ConcurrentDictionary<Type, IPostTransactionEventHandlerWrapper> EventHandlerWrappers = new();

    public void Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
            return;

        using var activity = HerrGeneralDiagnostics.StartActivity(
            HerrGeneralDiagnostics.Activities.PostTransactionDispatch);

        activity?.SetTag(HerrGeneralDiagnostics.Tags.EventsCount, events.Count);
        activity?.SetTag(HerrGeneralDiagnostics.Tags.ThreadId, Environment.CurrentManagedThreadId);

        HerrGeneralDiagnostics.EventsTotal.Add(events.Count,
            new KeyValuePair<string, object?>("herrgeneral.stage", "PostTransaction"));

        foreach (var eventToDispatch in events) 
            DispatchSingle(eventToDispatch);
    }

    private void DispatchSingle(object eventToDispatch)
    {
        var wrapper = EventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), CreateWrapper);
        wrapper.Handle(eventToDispatch, serviceProvider);
    }

    private static IPostTransactionEventHandlerWrapper CreateWrapper(Type eventType) =>
        (IPostTransactionEventHandlerWrapper)Activator.CreateInstance(typeof(PostTransactionEventHandlerWrapper<>).MakeGenericType(eventType))!;
}
