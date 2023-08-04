using System.Collections.Concurrent;
using HerrGeneral.Contracts.WriteSide;
using HerrGeneral.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.ReadSide.Dispatcher;

internal class ReadSideEventDispatcher : EventDispatcherBase, IEventDispatcher, IAddEventToDispatch
{
    private readonly ILogger<ReadSideEventDispatcher> _logger;
    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    private readonly ConcurrentDictionary<Guid, List<IEvent>> _eventsToDispatch = new();

    public ReadSideEventDispatcher(IServiceProvider serviceProvider) : 
        this(serviceProvider, NullLogger<ReadSideEventDispatcher>.Instance)
    {
    }
    
    public ReadSideEventDispatcher(IServiceProvider serviceProvider, ILogger<ReadSideEventDispatcher> logger) : base(serviceProvider) => 
        _logger = logger;

    public void AddEventToDispatch(IEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        _eventsToDispatch
            .AddOrUpdate(@event.SourceCommandId, new List<IEvent> { @event }, (commandIdUpdate, events) =>
            {
                events.Add(@event);
                return events;
            });
    }

    private IEnumerable<IEvent> GetAndRemove(Guid commandId)
    {
        if (commandId.IsEmpty())
            throw new ArgumentNullException(nameof(commandId));

        return _eventsToDispatch.TryRemove(commandId, out var events)
            ? events
            : Enumerable.Empty<IEvent>();
    }

    public async Task Dispatch(Guid commandId, CancellationToken cancellationToken)
    {
        var eventsToPublish = GetAndRemove(commandId).OrderBy(@event => @event.DateTimeEventOccurred).ToList();

        _logger.LogReadSidePublishStart(eventsToPublish.Count);

        foreach (var eventToDispatch in eventsToPublish)
        {
            _logger.LogPublishEventOnReadSide(eventToDispatch);
            await Dispatch(eventToDispatch, cancellationToken);
        }
    }
}