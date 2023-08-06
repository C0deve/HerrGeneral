using HerrGeneral.Contracts;

namespace HerrGeneral.Core.ReadSide.Dispatcher;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(IEvent @event);
}