using HerrGeneral.Contracts;

namespace HerrGeneral.Core.ReadSide;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(IEvent @event);
}