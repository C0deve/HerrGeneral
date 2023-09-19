namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record AChangeCommandWithoutHandler(string Name, Guid AggregateId) : ChangeAggregate<Person>(AggregateId);