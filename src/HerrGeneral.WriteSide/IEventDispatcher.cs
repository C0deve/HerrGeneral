using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

/// <summary>
/// Interface for event dispatcher
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatch an event
    /// </summary>
    /// <param name="eventToDispatch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken);
}