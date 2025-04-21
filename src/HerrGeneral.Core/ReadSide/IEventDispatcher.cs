namespace HerrGeneral.Core.ReadSide;

internal interface IEventDispatcher
{
    void Dispatch(Guid commandId, CancellationToken cancellationToken);
}