using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record AChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheThing>(AggregateId), INoHandlerChange<TheThing>;
public record ASecondChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheThing>(AggregateId), INoHandlerChange<TheThing>;
public record AThirdChangeCommandWithoutHandler(string Name, Guid AggregateId) : Change<TheThing>(AggregateId), INoHandlerChange<TheThing>;