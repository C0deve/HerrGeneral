namespace HerrGeneral.DDD;

/// <summary>
/// Defines a handler interface for multi-aggregates changes.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate the handler operates on.</typeparam>
/// <typeparam name="TCommand">The type of command the handler processes.</typeparam>
public interface IChangeMultiHandler<out TAggregate, in TCommand>
    where TAggregate : Aggregate<TAggregate>
{
    /// <summary>
    /// Returns the changed aggregates.
    /// The save and event dispatching is handled by HerrGeneral.
    /// </summary>
    /// <param name="command"></param>
    /// <returns>The changed aggregates.</returns>
    IEnumerable<TAggregate> Handle(TCommand command);
}