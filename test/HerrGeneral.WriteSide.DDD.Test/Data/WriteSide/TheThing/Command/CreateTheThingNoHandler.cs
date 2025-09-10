namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record CreateTheThingNoHandler(string Name) : Create<TheThing>;
public record CreateTheThingNoHandlerWithFailure : Create<TheThing>;