using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventDispatcher(
    IServiceProvider serviceProvider,
    IAddEventToDispatch readSideEventDispatcher,
    CommandExecutionTracer? commandExecutionTracer = null)
    : EventDispatcherBase(serviceProvider)
{
    protected override Type WrapperOpenType => typeof(WriteSideEventHandlerWrapper<>);

    public override void Dispatch(object eventToDispatch)
    {
        commandExecutionTracer?.PublishEventOnWriteSide(eventToDispatch);

        base.Dispatch(eventToDispatch);
        readSideEventDispatcher.AddEventToDispatch(eventToDispatch);
    }
}