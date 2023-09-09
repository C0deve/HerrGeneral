using System.Collections.Concurrent;
using HerrGeneral.Contracts;
using HerrGeneral.Core.Logger;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal class ReadSideEventDispatcher : EventDispatcherBase, IEventDispatcher, IAddEventToDispatch
{
    private readonly ILogger<ReadSideEventDispatcher> _logger;
    private readonly CommandLogger _commandLogger;
    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    private readonly ConcurrentDictionary<Guid, List<IEvent>> _eventsToDispatch = new();
    
    public ReadSideEventDispatcher(IServiceProvider serviceProvider, ILogger<ReadSideEventDispatcher> logger, CommandLogger commandLogger) : base(serviceProvider)
    {
        _logger = logger;
        _commandLogger = commandLogger;
    }
    
    public ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandLogger commandLogger) : base(serviceProvider)
    {
        _logger = NullLogger<ReadSideEventDispatcher>.Instance;
        _commandLogger = commandLogger;
    }

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
        var stringBuilder = _logger.IsEnabled(LogLevel.Debug)
            ? _commandLogger.GetStringBuilder(commandId)
            : null;

        var eventsToPublish = GetAndRemove(commandId).OrderBy(@event => @event.DateTimeEventOccurred).ToList();
        stringBuilder?.StartPublishEventsOnReadSide(eventsToPublish.Count);
        foreach (var eventToDispatch in eventsToPublish)
        {
            stringBuilder?.PublishEventOnReadSide(eventToDispatch);
            await Dispatch(eventToDispatch, cancellationToken);
        }
    }
}