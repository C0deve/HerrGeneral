namespace HerrGeneral;

/// <summary>
/// Defines a projection handler executed post-transaction after the database transaction has committed.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
public interface IHandlePostProjection<in TEvent>
{
    /// <summary>
    /// Updates eventual consistency projections with the specified event.
    /// </summary>
    /// <param name="event">The domain event being processed.</param>
    void Handle(TEvent @event);
}
