using System.Collections.Concurrent;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventDispatcher(
    IServiceProvider serviceProvider,
    CommandExecutionTracer? commandExecutionTracer = null)
{
    private static Type WrapperOpenType => typeof(WriteSideEventHandlerWrapper<>);
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _eventHandlerWrappers = new();

    /// <summary>
    /// Dispatches the events to their respective handlers in FIFO order.
    /// Processes each event through registered event handlers and collects any additional events
    /// generated during handling for further processing.
    /// </summary>
    /// <param name="events">The collection of events to be dispatched</param>
    /// <returns>A collection of all events that were processed</returns>
    public IReadOnlyList<object> Dispatch(IReadOnlyList<object> events)
    {
        if (events.Count == 0)
        {
            return [];
        }

        // Enqueue all events
        Queue<object> eventQueue = new(events);
        commandExecutionTracer?.StartPublishEventOnWriteSide();

        // Process in FIFO
        var processedEvents = new List<object>(events.Count);
        while (eventQueue.TryDequeue(out var evt))
        {
            commandExecutionTracer?.PublishEventOnWriteSide(evt);
            foreach (var newEventToDispatch in Dispatch(evt))
                eventQueue.Enqueue(newEventToDispatch);
            processedEvents.Add(evt);
        }

        return processedEvents;
    }

    /// <summary>
    /// Dispatch the event using an instance of <see cref="WrapperOpenType"/>
    /// </summary>
    /// <param name="eventToDispatch"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private IReadOnlyList<object> Dispatch(object eventToDispatch)
    {
        var wrapper = _eventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), eventTypeInput =>
        {
            var wrapperType = WrapperOpenType.MakeGenericType(eventTypeInput);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {eventToDispatch.GetType()}");
            return (IEventHandlerWrapper)wrapper;
        });

        return wrapper.Handle(eventToDispatch, serviceProvider);
    }
}