namespace HerrGeneral.DDD;

/// <summary>
/// Represents an individual change request applied to a specific set of aggregates with typed key.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of aggregate to which the change request applies.
/// </typeparam>
/// <typeparam name="TKey">
/// The type of the aggregate key.
/// </typeparam>
/// <param name="AggregateIds">
/// The identifiers of the aggregates to which this change request will be applied.
/// </param>
/// <param name="UpdateAction">
/// A function that describes the modification to be applied to each aggregate.
/// </param>
public record ChangeRequest<TAggregate, TKey>(IReadOnlyCollection<TKey> AggregateIds, Func<TAggregate, TAggregate> UpdateAction)
    where TAggregate : IAggregate
    where TKey : notnull;

/// <summary>
/// Represents an individual change request applied to a specific set of aggregates with Guid key.
/// </summary>
/// <typeparam name="TAggregate">
/// The type of aggregate to which the change request applies.
/// </typeparam>
/// <param name="AggregateIds">
/// The identifiers of the aggregates to which this change request will be applied.
/// </param>
/// <param name="UpdateAction">
/// A function that describes the modification to be applied to each aggregate.
/// </param>
public record ChangeRequest<TAggregate>(IReadOnlyCollection<Guid> AggregateIds, Func<TAggregate, TAggregate> UpdateAction)
    : ChangeRequest<TAggregate, Guid>(AggregateIds, UpdateAction)
    where TAggregate : IAggregate;