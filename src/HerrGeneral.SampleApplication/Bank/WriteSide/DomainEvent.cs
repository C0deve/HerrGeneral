using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public record DomainEvent<TAggregate, TKey>(Guid SourceCommandId, TKey AggregateId) : 
    IDomainEvent<TAggregate, TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}