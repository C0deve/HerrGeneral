using System.Text;
using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public abstract class EventBase : IEvent
{
    protected EventBase(Guid sourceCommandId, Guid aggregateId) : this(sourceCommandId, aggregateId, DateTime.Now){}
    protected EventBase(Guid sourceCommandId, Guid aggregateId, DateTime dateTimeEventOccurred)
    {
        EventId = Guid.NewGuid();
        SourceCommandId = sourceCommandId;
        AggregateId = aggregateId;
        DateTimeEventOccurred = dateTimeEventOccurred;
    }

    public virtual StringBuilder Log(StringBuilder sb, string indent = "") =>
        sb
            .AppendLine($"{indent}-- Event<{GetType()}>")
            .AppendLine($"{indent}-- Occured<{DateTimeEventOccurred:g}>");

    public DateTime DateTimeEventOccurred { get; }
    public Guid EventId { get; }
    public Guid SourceCommandId { get; }
    public Guid AggregateId { get; }
}