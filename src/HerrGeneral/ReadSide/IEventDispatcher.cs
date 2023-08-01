namespace HerrGeneral.ReadSide;

internal interface IEventDispatcher
{
    Task Dispatch(Guid commandId, CancellationToken cancellationToken);
}