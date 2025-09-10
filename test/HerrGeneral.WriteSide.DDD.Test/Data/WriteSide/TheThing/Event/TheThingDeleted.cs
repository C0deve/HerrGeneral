namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

/// <summary>
/// Event raised when TheThing is deleted
/// </summary>
public record TheThingDeleted(string Name, Guid SourceCommandId, Guid AggregateId) : 
    DomainEvent<TheThing>(SourceCommandId, AggregateId);
