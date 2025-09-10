namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

/// <summary>
/// Event raised when AnotherThing is created
/// </summary>
public record AnotherThingCreated(
    string Name, 
    Guid ParentAggregateId, 
    Guid SourceCommandId, 
    Guid AggregateId) : DomainEvent<AnotherThing>(SourceCommandId, AggregateId);
