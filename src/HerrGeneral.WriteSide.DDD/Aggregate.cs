using HerrGeneral.WriteSide.DDD.Exception;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Aggregate implementation with IDomainEvent publication
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Aggregate<T> : IAggregate where T : Aggregate<T>
{
    private readonly List<IDomainEvent<T>> _newEvents = [];

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="id"></param>
    protected Aggregate(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));
        Id = id;
    }

    /// <summary>
    /// Unique Id of the Aggregate 
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// All new IDomainEvent to dispatch
    /// </summary>
    public IEnumerable<IDomainEvent<T>> NewEvents
    {
        get
        {
            lock (_newEvents)
            {
                return _newEvents.ToList();
            }
        }
    }

    /// <summary>
    /// Clear all IDomainEvents waiting for dispatch
    /// </summary>
    /// <returns></returns>
    internal T ClearNewEvents()
    {
        lock (_newEvents)
        {
            _newEvents.Clear();
        }

        return (T)this;
    }

    /// <summary>
    /// Add a IDomainEvent to dispatch
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="IdMismatchOnEventEmit{T}"></exception>
    protected virtual T Emit(IDomainEvent<T> @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        if (@event.AggregateId != Id)
            throw new IdMismatchOnEventEmit<T>(this, @event);

        lock (_newEvents)
            _newEvents.Add(@event);

        return (T)this;
    }
}