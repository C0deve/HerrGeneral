using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.Core.ReadSide.Dispatcher;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(IEvent @event);
}