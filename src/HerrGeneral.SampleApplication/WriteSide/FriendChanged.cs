using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public record FriendChanged(string Person, string FriendName, Guid SourceCommandId, Guid AggregateId) : IDomainEvent<Person>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}