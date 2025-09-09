namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record AChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheAggregate>(AggregateId);
public record ASecondChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheAggregate>(AggregateId);
public record AThirdChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheAggregate>(AggregateId);