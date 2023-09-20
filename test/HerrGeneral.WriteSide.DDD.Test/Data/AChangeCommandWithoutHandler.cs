namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record AChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<Person>(AggregateId);
public record ASecondChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<Person>(AggregateId);
public record AThirdChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<Person>(AggregateId);