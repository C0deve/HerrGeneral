namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

public record TheAggregateIsCreated(string Name, Guid SourceCommandId, Guid AggregateId) : 
    DomainEvent<TheAggregate>(SourceCommandId, AggregateId);