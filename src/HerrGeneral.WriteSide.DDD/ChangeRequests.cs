using System.Collections.ObjectModel;

namespace HerrGeneral.DDD;

/// <summary>
/// Represents a collection of change requests for a specific aggregate type with typed key.
/// This class is designed to collect and manage modifications applicable to aggregates of type TAggregate.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate to which change requests apply.
/// </typeparam>
/// <typeparam name="TKey">
/// The type of the aggregate key.
/// </typeparam>
public class ChangeRequests<TAggregate, TKey>
    where TAggregate : IAggregate
    where TKey : notnull
{
    private readonly List<ChangeRequest<TAggregate, TKey>> _actions = [];

    /// <summary>
    /// Adds a change request to the collection for the specified aggregate, associating it with the provided IDs.
    /// </summary>
    /// <param name="action">The function representing the modification to be applied to the aggregate of type <typeparamref name="TAggregate"/>.</param>
    /// <param name="ids">An array of unique identifiers representing the aggregates this change request targets.</param>
    /// <returns>
    /// The current instance of <see cref="ChangeRequests{TAggregate, TKey}"/> for method chaining.
    /// </returns>
    public ChangeRequests<TAggregate, TKey> Add(Func<TAggregate, TAggregate> action, params TKey[] ids)
    {
        _actions.Add(new ChangeRequest<TAggregate, TKey>(ids, action));
        return this;
    }

    internal ReadOnlyCollection<ChangeRequest<TAggregate, TKey>> Actions => _actions.ToArray().AsReadOnly();
}

/// <summary>
/// Represents a collection of change requests for a specific aggregate type with Guid key.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate to which change requests apply.
/// </typeparam>
public class ChangeRequests<TAggregate> : ChangeRequests<TAggregate, Guid>
    where TAggregate : IAggregate
{
    /// <summary>
    /// Adds a change request to the collection for the specified aggregate, associating it with the provided IDs.
    /// </summary>
    /// <param name="action">The function representing the modification to be applied to the aggregate of type <typeparamref name="TAggregate"/>.</param>
    /// <param name="ids">An array of unique identifiers representing the aggregates this change request targets.</param>
    /// <returns>
    /// The current instance of <see cref="ChangeRequests{TAggregate}"/> for method chaining.
    /// </returns>
    public new ChangeRequests<TAggregate> Add(Func<TAggregate, TAggregate> action, params Guid[] ids)
    {
        base.Add(action, ids);
        return this;
    }
}