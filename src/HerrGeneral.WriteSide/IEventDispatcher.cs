namespace HerrGeneral.WriteSide;

/// <summary>
/// Interface for event dispatcher
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatch an event
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="eventToDispatch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    void Dispatch(Guid commandId, object eventToDispatch, CancellationToken cancellationToken);
}