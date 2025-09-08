namespace HerrGeneral.Core.DDD.Exception;

/// <summary>
/// Aggregate not found implementation
/// </summary>
/// <typeparam name="T"></typeparam>
public class AggregateNotFound<T> : System.Exception //where T : EventSourcingAggregate<T>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public AggregateNotFound(Guid id) : base($"Unable to find aggregate n°'{id}' of type '{typeof(T).FullName}'.")
    {}

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public AggregateNotFound(Guid id, string message) : base($"{message} \nUnable to find aggregate n°'{id}' of type '{typeof(T).FullName}'.")
    {
            
    }
}