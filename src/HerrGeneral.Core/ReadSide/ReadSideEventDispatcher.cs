using System.Collections.Concurrent;
using HerrGeneral.Core.WriteSide;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal sealed class ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandExecutionTracer? commandExecutionTracer = null)
{
    private static Type WrapperOpenType => typeof(EventHandlerWrapper<>);
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _eventHandlerWrappers = new();
    

    public void Dispatch(params IEnumerable<object> events)
    {
        var eventToDispatches = events as object[] ?? events.ToArray();
        commandExecutionTracer?.StartPublishEventsOnReadSide(eventToDispatches.Length);
        foreach (var eventToDispatch in eventToDispatches)
        {
            commandExecutionTracer?.PublishEventOnReadSide(eventToDispatch);
            Dispatch(eventToDispatch);
        }
    }

    /// <summary>
    /// Dispatch the event using an instance of <see cref="WrapperOpenType"/>
    /// </summary>
    /// <param name="eventToDispatch"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void Dispatch(object eventToDispatch)
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