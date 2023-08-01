using HerrGeneral.Contracts;

namespace HerrGeneral.ReadSide.Dispatcher;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(IEvent @event);
}