namespace HerrGeneral.DDD;

/// <summary>
/// Defines a handler interface for multi-aggregates changes with typed key.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate the handler operates on.</typeparam>
/// <typeparam name="TCommand">The type of command the handler processes.</typeparam>
/// <typeparam name="TKey">The type of the aggregate key.</typeparam>
public interface IChangeMultiHandler<out TAggregate, in TCommand, TKey>
    where TAggregate : Aggregate<TAggregate, TKey>
    where TKey : notnull
{
    /// <summary>
    /// Returns the changed aggregates.
    /// The save and event dispatching is handled by HerrGeneral.
    /// </summary>
    /// <param name="command"></param>
    /// <returns>The changed aggregates.</returns>
    IEnumerable<TAggregate> Handle(TCommand command);
}

/// <summary>
/// Defines a handler interface for multi-aggregates changes with Guid key.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate the handler operates on.</typeparam>
/// <typeparam name="TCommand">The type of command the handler processes.</typeparam>
public interface IChangeMultiHandler<out TAggregate, in TCommand> : IChangeMultiHandler<TAggregate, TCommand, Guid>
    where TAggregate : Aggregate<TAggregate, Guid>
{
}