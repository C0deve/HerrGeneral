using HerrGeneral.Contracts;
using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal class EventDispatcher : EventDispatcherBase, HerrGeneral.WriteSide.IEventDispatcher
{
    private readonly IAddEventToDispatch _readSideEventDispatcher;

    public EventDispatcher(IServiceProvider serviceProvider, IAddEventToDispatch readSideEventDispatcher) : base(serviceProvider) => 
        _readSideEventDispatcher = readSideEventDispatcher;

    protected override Type WrapperOpenType => typeof(EventHandlerWrapper<>);

    public override async Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken)
    {
        await base.Dispatch(eventToDispatch, cancellationToken);
        _readSideEventDispatcher.AddEventToDispatch(eventToDispatch);
    }
}