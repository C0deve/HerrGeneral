namespace HerrGeneral.DDD;

/// <summary>
/// Represents an individual change request applied to a specific set of aggregates.
/// This is defined by a collection of aggregate identifiers and a transformation function.
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
public record ChangeRequest<TAggregate>(IReadOnlyCollection<Guid> AggregateIds, Func<TAggregate, TAggregate> UpdateAction);