using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public record DomainEvent<TAggregate>(Guid SourceCommandId, Guid AggregateId) : 
    IDomainEvent<TAggregate> where TAggregate : IAggregate
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}