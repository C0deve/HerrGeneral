using System.Collections.ObjectModel;

namespace HerrGeneral.DDD;

/// <summary>
/// Represents a collection of change requests for a specific aggregate type.
/// This class is designed to collect and manage modifications applicable to aggregates of type TAggregate.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of the aggregate to which change requests apply.
/// </typeparam>
public sealed class ChangeRequests<TAggregate>
{
    private readonly List<ChangeRequest<TAggregate>> _actions = [];

    /// <summary>
    /// Adds a change request to the collection for the specified aggregate, associating it with the provided IDs.
    /// </summary>
    /// <param name="action">The function representing the modification to be applied to the aggregate of type <typeparamref name="TAggregate"/>.</param>
    /// <param name="ids">An array of unique identifiers representing the aggregates this change request targets.</param>
    /// <returns>
    /// The current instance of <see cref="ChangeRequests{TAggregate}"/> for method chaining.
    /// </returns>
    public ChangeRequests<TAggregate> Add(Func<TAggregate, TAggregate> action, params Guid[] ids)
    {
        _actions.Add(new ChangeRequest<TAggregate>(ids, action));
        return this;
    }

    internal ReadOnlyCollection<ChangeRequest<TAggregate>> Actions => _actions.ToArray().AsReadOnly();

}