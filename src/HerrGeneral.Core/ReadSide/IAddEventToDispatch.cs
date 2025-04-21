namespace HerrGeneral.Core.ReadSide;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(Guid commandId, object @event);
}