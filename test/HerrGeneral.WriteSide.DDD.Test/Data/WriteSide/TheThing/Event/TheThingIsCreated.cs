namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

public record TheThingIsCreated(string Name, Guid SourceCommandId, Guid AggregateId) : 
    DomainEvent<TheThing>(SourceCommandId, AggregateId);