namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

public record TheThingHasChanged(string Name, Guid SourceCommandId, Guid AggregateId) : 
    DomainEvent<TheThing>(SourceCommandId, AggregateId);