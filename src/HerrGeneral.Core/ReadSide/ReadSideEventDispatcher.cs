using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Strongly inspired from https://github.com/jbogard/MediatR

namespace HerrGeneral.Core.ReadSide;

internal class ReadSideEventDispatcher : EventDispatcherBase, IAddEventToDispatch
{
    private readonly ILogger<ReadSideEventDispatcher> _logger;
    private readonly CommandExecutionTracer _commandExecutionTracer;
    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    private readonly List<object> _eventsToDispatch = [];

    public ReadSideEventDispatcher(IServiceProvider serviceProvider, ILogger<ReadSideEventDispatcher> logger, CommandExecutionTracer commandExecutionTracer) : base(serviceProvider)
    {
        _logger = logger;
        _commandExecutionTracer = commandExecutionTracer;
    }

    public ReadSideEventDispatcher(IServiceProvider serviceProvider, CommandExecutionTracer commandExecutionTracer) : base(serviceProvider)
    {
        _logger = NullLogger<ReadSideEventDispatcher>.Instance;
        _commandExecutionTracer = commandExecutionTracer;
    }

    public void AddEventToDispatch(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _eventsToDispatch.Add(@event);
    }

    public void Dispatch()
    {
        var tracer = _logger.IsEnabled(LogLevel.Debug)
            ? _commandExecutionTracer
            : null;

        tracer?.StartPublishEventsOnReadSide(_eventsToDispatch.Count);
        foreach (var eventToDispatch in _eventsToDispatch)
        {
            tracer?.PublishEventOnReadSide(eventToDispatch);
            Dispatch(eventToDispatch);
        }
    }
}