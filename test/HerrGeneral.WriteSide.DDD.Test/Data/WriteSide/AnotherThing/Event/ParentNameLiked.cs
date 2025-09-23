namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

public record ParentNameLiked(Guid CommandId, Guid Id) : DomainEvent<AnotherThing>(CommandId, Id);