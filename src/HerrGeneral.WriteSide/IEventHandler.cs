namespace HerrGeneral.WriteSide;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TEvent">The type of notification being handled</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// Handle a notification and optionally produce additional events.
    /// This method processes the incoming event and may generate new events as a result
    /// of the business logic execution. The returned events will be dispatched in FIFO order.
    /// </summary>
    /// <param name="notification">The notification event to be processed</param>
    /// <returns>A collection of events generated as a result of handling the notification, or an empty collection if no events are produced</returns>
    IEnumerable<object> Handle(TEvent notification);
}