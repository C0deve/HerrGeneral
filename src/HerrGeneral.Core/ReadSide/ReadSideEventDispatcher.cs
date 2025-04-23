using System.Collections.Concurrent;
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

    private readonly ConcurrentDictionary<Guid, List<object>> _eventsToDispatch = new();

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

    public void AddEventToDispatch(Guid commandId, object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _eventsToDispatch
            .AddOrUpdate(commandId, [@event], (_, events) =>
            {
                events.Add(@event);
                return events;
            });
    }

    private IEnumerable<object> GetAndRemove(Guid commandId)
    {
        if (commandId.IsEmpty())
            throw new ArgumentNullException(nameof(commandId));

        return _eventsToDispatch.TryRemove(commandId, out var events)
            ? events
            : Enumerable.Empty<object>();
    }

    public void Dispatch(Guid commandId, CancellationToken cancellationToken)
    {
        var stringBuilder = _logger.IsEnabled(LogLevel.Debug)
            ? _commandLogger.GetStringBuilder(commandId)
            : null;

        var eventsToPublish = GetAndRemove(commandId).ToList();
        stringBuilder?.StartPublishEventsOnReadSide(eventsToPublish.Count);
        foreach (var eventToDispatch in eventsToPublish)
        {
            stringBuilder?.PublishEventOnReadSide(eventToDispatch);
            Dispatch(commandId, eventToDispatch, cancellationToken);
        }
    }
}