namespace HerrGeneral.DDD;

/// <summary>
/// Interface for aggregate repository with typed key
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IAggregateRepository<T, in TKey>
    where T : IAggregate
    where TKey : notnull
{
    /// <summary>
    /// Find the aggregate by id 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T Get(TKey id);

    /// <summary>
    /// Save an aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    void Save(T aggregate);
}

/// <summary>
/// Interface for aggregate repository with Guid key
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAggregateRepository<T> : IAggregateRepository<T, Guid>
    where T : IAggregate
{
}