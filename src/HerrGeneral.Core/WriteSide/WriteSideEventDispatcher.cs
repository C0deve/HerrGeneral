using HerrGeneral.Core.Logger;
using HerrGeneral.Core.ReadSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventDispatcher : EventDispatcherBase
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
        _logger =  NullLogger<WriteSideEventDispatcher>.Instance;
        _commandLogger = commandLogger;
    }

    protected override Type WrapperOpenType => typeof(WriteSideEventHandlerWrapper<>);

    public override void Dispatch(UnitOfWorkId unitOfWorkId, object eventToDispatch, CancellationToken cancellationToken)
    {
        if(_logger.IsEnabled(LogLevel.Debug))
            _commandLogger
                .GetStringBuilder(unitOfWorkId)
                .PublishEventOnWriteSide(eventToDispatch);

        base.Dispatch(unitOfWorkId, eventToDispatch, cancellationToken);
        _readSideEventDispatcher.AddEventToDispatch(unitOfWorkId, eventToDispatch);
    }
}