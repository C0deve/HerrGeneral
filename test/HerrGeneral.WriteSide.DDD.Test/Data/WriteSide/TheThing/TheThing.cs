using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;

public class TheThing : Aggregate<TheThing>
{
    public TheThing(CreateTheThingNoHandler command, Guid aggregateId)
        : this(aggregateId, command.Name, command.Id)
    {
    }

    public TheThing(Guid id, string name, Guid commandId) : base(id)
    {
        Name = name;
        Emit(new TheThingIsCreated(name, commandId, Id));
    }

    public string Name { get; }

    public TheThing AddFriend(string friendName, Guid sourceCommandId) =>
        Emit(new TheThingHasChanged(friendName, sourceCommandId, Id));

    public TheThing AddFriendWithDifferentAggregateId(string friendName, Guid sourceCommandId) =>
        Emit(new TheThingHasChanged(friendName, sourceCommandId, Guid.NewGuid()));

    // ReSharper disable once UnusedMember.Global
    internal TheThing Execute(AChangeCommandWithoutHandler changeCommand) =>
        Emit(new TheThingHasChanged(changeCommand.Name, changeCommand.Id, Id));

    // ReSharper disable once UnusedMember.Global
    internal TheThing Execute(ASecondChangeCommandWithoutHandler changeCommand) =>
        Emit(new TheThingHasChanged(changeCommand.Name, changeCommand.Id, Id));
}