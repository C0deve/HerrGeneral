namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record CreateTheAggregateNoHandler(string Name, string Friend) : Create<TheAggregate>;
public record CreateTheAggregateNoHandlerWithFailure : Create<TheAggregate>;