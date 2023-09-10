using HerrGeneral.Contracts;

namespace HerrGeneral.Test;

/// <summary>
/// Event implementation.
/// </summary>
public abstract record EventBase : IEvent
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="sourceCommandId"></param>
    /// <param name="aggregateId"></param>
    protected EventBase(Guid sourceCommandId, Guid aggregateId) : this(sourceCommandId, aggregateId, DateTime.Now){}
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="sourceCommandId"></param>
    /// <param name="aggregateId"></param>
    /// <param name="dateTimeEventOccurred"></param>
    protected EventBase(Guid sourceCommandId, Guid aggregateId, DateTime dateTimeEventOccurred)
    {
        EventId = Guid.NewGuid();
        SourceCommandId = sourceCommandId;
        AggregateId = aggregateId;
        DateTimeEventOccurred = dateTimeEventOccurred;
    }

    /// <summary>
    /// Execution date of the event.
    /// </summary>
    public DateTime DateTimeEventOccurred { get; }
    
    /// <summary>
    /// Id of the event.
    /// </summary>
    public Guid EventId { get; }
    
    /// <summary>
    /// Id of the command who triggered the event.
    /// </summary>
    public Guid SourceCommandId { get; }
    
    /// <summary>
    ///  Id of the aggregate who triggered the event.
    /// </summary>
    public Guid AggregateId { get; }
}