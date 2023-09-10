namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Interface for aggregate repository
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAggregateRepository<T> where T : Aggregate<T>
{
    /// <summary>
    /// Find the aggregate with by id 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceCommandId"></param>
    /// <returns></returns>
    T? Get(Guid id, Guid sourceCommandId);
    
    /// <summary>
    /// Save an aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="sourceCommandId"></param>
    void Save(T aggregate, Guid sourceCommandId);
}