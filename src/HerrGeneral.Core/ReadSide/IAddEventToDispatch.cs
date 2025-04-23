namespace HerrGeneral.Core.ReadSide;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(UnitOfWorkId commandId, object @event);
}