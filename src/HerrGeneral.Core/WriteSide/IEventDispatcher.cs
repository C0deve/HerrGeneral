using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.Core.WriteSide;

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