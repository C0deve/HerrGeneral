namespace HerrGeneral.Core.ReadSide;

internal interface IEventDispatcher
{
    Task Dispatch(Guid commandId, CancellationToken cancellationToken);
}