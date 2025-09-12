using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record CreateTheThingNoHandler(string Name) : Create<TheThing>, INoHandlerCreate<TheThing>;

public record CreateTheThingNoHandlerWithFailure : Create<TheThing>, INoHandlerCreate<TheThing>;