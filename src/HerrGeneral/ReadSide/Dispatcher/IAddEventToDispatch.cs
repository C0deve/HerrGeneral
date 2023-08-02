using HerrGeneral.Contracts;
using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.ReadSide.Dispatcher;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(IEvent @event);
}