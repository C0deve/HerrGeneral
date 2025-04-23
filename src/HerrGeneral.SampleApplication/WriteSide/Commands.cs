using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public record CreatePerson(string Name, string MyFriend) : Create<Person>;
public record SetFriend(Guid AggregateId, string Friend) : Change<Person>(AggregateId);