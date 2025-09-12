namespace HerrGeneral.DDD;

/// <summary>
/// Marker interface for aggregate
/// </summary>
public interface IAggregate
{
    /// <summary>
    /// Unique Id of the Aggregate 
    /// </summary>
    Guid Id { get; }
}