namespace HerrGeneral.DDD.Exception;

/// <summary>
/// Aggregate not found exception with typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class AggregateNotFound<TAggregate, TKey> : System.Exception
    where TKey : notnull
{
    /// <summary>
    /// Identifier of the aggregate that was not found.
    /// </summary>
    public TKey Id { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public AggregateNotFound(TKey id) : base($"Unable to find aggregate n°'{id}' of type '{typeof(TAggregate).FullName}'.")
    {
        Id = id;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public AggregateNotFound(TKey id, string message) : base($"{message} \nUnable to find aggregate n°'{id}' of type '{typeof(TAggregate).FullName}'.")
    {
        Id = id;
    }
}

/// <summary>
/// Aggregate not found exception with object/Guid key fallback
/// </summary>
/// <typeparam name="T"></typeparam>
public class AggregateNotFound<T> : AggregateNotFound<T, object>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    public AggregateNotFound(object id) : base(id)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    public AggregateNotFound(object id, string message) : base(id, message)
    {
    }
}