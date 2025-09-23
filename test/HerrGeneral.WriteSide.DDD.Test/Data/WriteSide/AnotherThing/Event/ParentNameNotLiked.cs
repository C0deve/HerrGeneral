namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

public record ParentNameNotLiked(Guid CommandId, Guid Id) : DomainEvent<AnotherThing>(CommandId, Id);