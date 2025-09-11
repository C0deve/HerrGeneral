namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Interface for aggregate repository
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAggregateRepository<T> where T : IAggregate
{
    /// <summary>
    /// Find the aggregate with by id 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T Get(Guid id);

    /// <summary>
    /// Save an aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    void Save(T aggregate);
}