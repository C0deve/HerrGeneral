using System.Collections.Concurrent;
using HerrGeneral.Core.WriteSide;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal sealed class ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandExecutionTracer? commandExecutionTracer = null)
{
    private static Type WrapperOpenType => typeof(EventHandlerWrapper<>);
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _eventHandlerWrappers = new();
    

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
    /// Dispatch the event using an instance of <see cref="WrapperOpenType"/>
    /// </summary>
    /// <param name="eventToDispatch"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void DispatchSingle(object eventToDispatch)
    {
        var wrapper = _eventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), eventTypeInput =>
        {
            var wrapperType = WrapperOpenType.MakeGenericType(eventTypeInput);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {eventToDispatch.GetType()}");
            return (IEventHandlerWrapper)wrapper;
        });

        wrapper.Handle(eventToDispatch, serviceProvider);
    }
}