namespace HerrGeneral.DDD;

/// <summary>
/// Represents a base class for planning changes to an aggregate with typed key.
/// This is an abstract class designed to manage and collect change requests for a specific type of aggregate.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate to which change requests apply. This type must implement the <see cref="IAggregate{TKey}"/> interface.
/// </typeparam>
/// <typeparam name="TKey">
/// The type of the aggregate key.
/// </typeparam>
public abstract class ChangesPlanner<TAggregate, TKey>
    where TAggregate : IAggregate
    where TKey : notnull
{
    /// <summary>
    /// Represents a collection of change requests for the corresponding aggregate type.
    /// This property simplifies the process of collecting and managing change requests for the aggregate type.
    /// </summary>
    protected ChangeRequests<TAggregate, TKey> Changes => new();
}

/// <summary>
/// Represents a base class for planning changes to an aggregate with Guid key.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate to which change requests apply.
/// </typeparam>
public abstract class ChangesPlanner<TAggregate>
    where TAggregate : IAggregate
{
    /// <summary>
    /// Represents a collection of change requests for the corresponding aggregate type.
    /// </summary>
    protected ChangeRequests<TAggregate> Changes => new();
}