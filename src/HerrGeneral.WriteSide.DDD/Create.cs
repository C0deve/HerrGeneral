namespace HerrGeneral.DDD;

/// <summary>
/// Command for aggregate creation with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract record Create<TAggregate, TKey> : CommandBase
    where TAggregate : IAggregate<TKey>
    where TKey : notnull;

/// <summary>
/// Command for aggregate creation with Guid key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public abstract record Create<TAggregate> : Create<TAggregate, Guid>
    where TAggregate : IAggregate<Guid>;