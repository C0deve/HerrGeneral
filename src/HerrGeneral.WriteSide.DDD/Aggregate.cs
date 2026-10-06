using HerrGeneral.DDD.Exception;

namespace HerrGeneral.DDD;

/// <summary>
/// Aggregate implementation with IDomainEvent publication and typed key
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract class Aggregate<TAggregate, TKey> : IAggregate<TKey>
    where TAggregate : Aggregate<TAggregate, TKey>
    where TKey : notnull
{
    private readonly List<IDomainEvent<TAggregate, TKey>> _newEvents = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    protected Aggregate(TKey id)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (id is Guid guid && guid == Guid.Empty) throw new ArgumentNullException(nameof(id));
        Id = id;
    }

    /// <summary>
    /// Unique Id of the Aggregate 
    /// </summary>
    public TKey Id { get; }

    /// <summary>
    /// All new IDomainEvent to dispatch
    /// </summary>
    public IReadOnlyList<IDomainEvent<TAggregate, TKey>> NewEvents
    {
        get
        {
            lock (_newEvents)
            {
                return [.._newEvents];
            }
        }
    }

    /// <summary>
    /// Clear all IDomainEvents waiting for dispatch
    /// </summary>
    /// <returns></returns>
    internal TAggregate ClearNewEvents()
    {
        lock (_newEvents)
        {
            _newEvents.Clear();
        }

        return (TAggregate)this;
    }

    /// <summary>
    /// Add an IDomainEvent to dispatch
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="IdMismatchOnEventEmit{TAggregate, TKey}"></exception>
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    protected virtual TAggregate Emit(IDomainEvent<TAggregate, TKey> domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (!EqualityComparer<TKey>.Default.Equals(domainEvent.AggregateId, Id))
            throw CreateIdMismatchException(domainEvent);

        lock (_newEvents)
            _newEvents.Add(domainEvent);

        return (TAggregate)this;
    }

    /// <summary>
    /// Creates the exception to throw when an event ID does not match aggregate ID.
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    protected virtual System.Exception CreateIdMismatchException(IDomainEvent<TAggregate, TKey> domainEvent) =>
        new IdMismatchOnEventEmit<TAggregate, TKey>(this, domainEvent);
}

/// <summary>
/// Aggregate implementation with Guid key for backwards compatibility
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="id"></param>
public abstract class Aggregate<TAggregate>(Guid id) : Aggregate<TAggregate, Guid>(id)
    where TAggregate : Aggregate<TAggregate>
{

    /// <inheritdoc />
    protected override System.Exception CreateIdMismatchException(IDomainEvent<TAggregate, Guid> domainEvent) =>
        new IdMismatchOnEventEmit<TAggregate>(this, (IDomainEvent<TAggregate>)domainEvent);
}