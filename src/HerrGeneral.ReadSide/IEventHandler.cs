
using HerrGeneral.Contracts;

namespace HerrGeneral.ReadSide;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TEvent">The type of notification being handled</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : IEvent
{
    /// <summary>
    /// Handles a notification
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Handle(TEvent notification, CancellationToken cancellationToken);
}