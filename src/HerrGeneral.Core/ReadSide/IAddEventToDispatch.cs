namespace HerrGeneral.Core.ReadSide;

internal interface IAddEventToDispatch
{
    void AddEventToDispatch(UnitOfWorkId unitOfWorkId, object @event);
}