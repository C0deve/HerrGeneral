namespace HerrGeneral.DDD;

/// <summary>
/// Defines a cross-aggregate handler executed within the active transaction to modify another aggregate with typed key.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
/// <typeparam name="TAggregate">The aggregate type being modified.</typeparam>
/// <typeparam name="TKey">The aggregate key type.</typeparam>
public interface IHandleCrossAggregate<in TEvent, TAggregate, TKey>
    where TAggregate : IAggregate
    where TKey : notnull
{
    /// <summary>
    /// Handles the event and returns change requests to be applied to the target aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event being handled.</param>
    /// <returns>A collection of change requests for the target aggregate.</returns>
    ChangeRequests<TAggregate, TKey> Handle(TEvent domainEvent);
}

/// <summary>
/// Defines a cross-aggregate handler executed within the active transaction to modify another aggregate with Guid key.
/// </summary>
/// <typeparam name="TEvent">The type of event being handled.</typeparam>
/// <typeparam name="TAggregate">The aggregate type being modified.</typeparam>
public interface IHandleCrossAggregate<in TEvent, TAggregate>
    where TAggregate : IAggregate
{
    /// <summary>
    /// Handles the event and returns change requests to be applied to the target aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event being handled.</param>
    /// <returns>A collection of change requests for the target aggregate.</returns>
    ChangeRequests<TAggregate> Handle(TEvent domainEvent);
}
