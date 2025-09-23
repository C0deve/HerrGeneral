namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

public record SubscribedToNameChangeNotifications(Guid CommandId, Guid Id) : DomainEvent<AnotherThing>(CommandId, Id);