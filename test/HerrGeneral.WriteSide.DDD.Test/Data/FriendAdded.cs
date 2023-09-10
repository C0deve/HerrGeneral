namespace HerrGeneral.WriteSide.DDD.Test.Data;

internal record FriendAdded(string Name, Guid SourceCommandId, Guid AggregateId) : IDomainEvent<Person>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}