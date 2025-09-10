
namespace HerrGeneral.ReadSide;

/// <summary>
/// Defines a handler that processes events to update read-side projections
/// </summary>
/// <typeparam name="TEvent">The type of event being handled</typeparam>
public interface IProjectionEventHandler<in TEvent>
{
    /// <summary>
    /// Processes the event to update the corresponding projection
    /// </summary>
    /// <param name="notification">The event to process</param>
    void Handle(TEvent notification);
}