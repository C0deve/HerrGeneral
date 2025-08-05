using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal class ReadSideEventDispatcher : EventDispatcherBase, IAddEventToDispatch
{
    private readonly ILogger<ReadSideEventDispatcher> _logger;
    private readonly CommandLogger _commandLogger;
    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    private readonly List<object> _eventsToDispatch = [];

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

    public void AddEventToDispatch(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _eventsToDispatch.Add(@event);
    }

    public void Dispatch()
    {
        var commandLogger = _logger.IsEnabled(LogLevel.Debug)
            ? _commandLogger
            : null;

        commandLogger?.StartPublishEventsOnReadSide(_eventsToDispatch.Count);
        foreach (var eventToDispatch in _eventsToDispatch)
        {
            commandLogger?.PublishEventOnReadSide(eventToDispatch);
            Dispatch(eventToDispatch);
        }
    }
}