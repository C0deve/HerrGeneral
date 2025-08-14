using HerrGeneral.Core.WriteSide;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal class ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandExecutionTracer? commandExecutionTracer = null)
    : EventDispatcherBase(serviceProvider), IAddEventToDispatch
{
    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    private readonly List<object> _eventsToDispatch = [];

    public void AddEventToDispatch(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _eventsToDispatch.Add(@event);
    }

    public void Dispatch()
    {
        commandExecutionTracer?.StartPublishEventsOnReadSide(_eventsToDispatch.Count);
        foreach (var eventToDispatch in _eventsToDispatch)
        {
            commandExecutionTracer?.PublishEventOnReadSide(eventToDispatch);
            Dispatch(eventToDispatch);
        }
    }
}