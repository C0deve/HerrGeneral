namespace HerrGeneral;

/// <summary>
/// Defines a projection handler executed within the active transaction before commit.
/// Fails and rolls back the transaction if projection update fails.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
public interface IHandleSyncProjection<in TEvent>
{
    /// <summary>
    /// Updates synchronous projections within the active transaction scope.
    /// </summary>
    /// <param name="event">The domain event being processed.</param>
    void Handle(TEvent @event);
}
