using HerrGeneral.Core.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventDispatcher : EventDispatcherBase
{
    private readonly IAddEventToDispatch _readSideEventDispatcher;
    private readonly ILogger<WriteSideEventDispatcher> _logger;
    private readonly CommandExecutionTracer _commandExecutionTracer;

    public WriteSideEventDispatcher(IServiceProvider serviceProvider, IAddEventToDispatch readSideEventDispatcher, ILogger<WriteSideEventDispatcher> logger, CommandExecutionTracer commandExecutionTracer) : base(serviceProvider)
    {
        _readSideEventDispatcher = readSideEventDispatcher;
        _logger = logger;
        _commandExecutionTracer = commandExecutionTracer;
    }
    public WriteSideEventDispatcher(IServiceProvider serviceProvider, IAddEventToDispatch readSideEventDispatcher, CommandExecutionTracer commandExecutionTracer) : base(serviceProvider)
    {
        _readSideEventDispatcher = readSideEventDispatcher;
        _logger =  NullLogger<WriteSideEventDispatcher>.Instance;
        _commandExecutionTracer = commandExecutionTracer;
    }

    protected override Type WrapperOpenType => typeof(WriteSideEventHandlerWrapper<>);

    public override void Dispatch(object eventToDispatch)
    {
        if(_logger.IsEnabled(LogLevel.Debug))
            _commandExecutionTracer
                .PublishEventOnWriteSide(eventToDispatch);

        base.Dispatch(eventToDispatch);
        _readSideEventDispatcher.AddEventToDispatch(eventToDispatch);
    }
}