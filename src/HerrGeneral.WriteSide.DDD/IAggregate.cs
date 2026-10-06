namespace HerrGeneral.DDD;

/// <summary>
/// Marker interface for aggregate
/// </summary>
public interface IAggregate;

/// <summary>
/// Interface for aggregate with typed key
/// </summary>
/// <typeparam name="TKey">Type of the aggregate key</typeparam>
public interface IAggregate<out TKey> : IAggregate where TKey : notnull
{
    /// <summary>
    /// Unique Id of the Aggregate 
    /// </summary>
    TKey Id { get; }
}