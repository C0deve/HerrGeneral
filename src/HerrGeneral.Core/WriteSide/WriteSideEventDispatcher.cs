using HerrGeneral.Contracts;
using HerrGeneral.Core.Logger;
using HerrGeneral.Core.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventDispatcher : EventDispatcherBase, HerrGeneral.WriteSide.IEventDispatcher
{
    private readonly IAddEventToDispatch _readSideEventDispatcher;
    private readonly ILogger<WriteSideEventDispatcher> _logger;
    private readonly CommandLogger _commandLogger;

    public WriteSideEventDispatcher(IServiceProvider serviceProvider, IAddEventToDispatch readSideEventDispatcher, ILogger<WriteSideEventDispatcher> logger, CommandLogger commandLogger) : base(serviceProvider)
    {
        _readSideEventDispatcher = readSideEventDispatcher;
        _logger = logger;
        _commandLogger = commandLogger;
    }
    public WriteSideEventDispatcher(IServiceProvider serviceProvider, IAddEventToDispatch readSideEventDispatcher, CommandLogger commandLogger) : base(serviceProvider)
    {
        _readSideEventDispatcher = readSideEventDispatcher;
        _logger =  NullLogger<WriteSideEventDispatcher>.Instance;;
        _commandLogger = commandLogger;
    }

    protected override Type WrapperOpenType => typeof(WriteSideEventHandlerWrapper<>);

    public override async Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken)
    {
        if(_logger.IsEnabled(LogLevel.Debug))
            _commandLogger
                .GetStringBuilder(eventToDispatch.SourceCommandId)
                .PublishEventOnWriteSide(eventToDispatch);
                
        await base.Dispatch(eventToDispatch, cancellationToken);
        _readSideEventDispatcher.AddEventToDispatch(eventToDispatch);
    }
}