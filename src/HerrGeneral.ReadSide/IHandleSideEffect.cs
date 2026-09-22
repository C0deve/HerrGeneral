namespace HerrGeneral;

/// <summary>
/// Defines a handler that executes post-transaction side-effects (e.g. notifications, emails, external message bus).
/// Executes strictly after the database transaction has committed.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
public interface IHandleSideEffect<in TEvent>
{
    /// <summary>
    /// Handles the event after transaction commit.
    /// </summary>
    /// <param name="event">The domain event being processed.</param>
    void Handle(TEvent @event);
}
