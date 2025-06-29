
namespace HerrGeneral.ReadSide;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TEvent">The type of notification being handled</typeparam>
public interface IEventHandler<in TEvent>
{
    /// <summary>
    /// Handles a notification
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    void Handle(TEvent notification);
}