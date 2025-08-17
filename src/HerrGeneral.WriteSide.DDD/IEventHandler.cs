namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TEvent">The type of notification being handled</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// Handle a notification
    /// </summary>
    /// <param name="notification">The notification</param>
    IEnumerable<object> Handle(TEvent notification);
}