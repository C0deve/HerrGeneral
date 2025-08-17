namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TEvent">The type of notification being handled</typeparam>
public interface ILocalEventHandler<in TEvent>
{
    /// <summary>
    /// Handle a notification
    /// </summary>
    /// <param name="notification">The notification</param>
    MyEventHandlerResult Handle(TEvent notification);
}