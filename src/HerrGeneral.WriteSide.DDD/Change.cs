namespace HerrGeneral.DDD;

/// <summary>
/// Command for editing an aggregate
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public record Change<TAggregate>(Guid AggregateId) : CommandBase
    where TAggregate : IAggregate;
