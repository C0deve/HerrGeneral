using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;

public class TheAggregate : Aggregate<TheAggregate>
{
    public TheAggregate(CreateTheAggregateNoHandler command, Guid aggregateId)
        : this(aggregateId, command.Name, command.Id)
    {
    }

    public TheAggregate(Guid id, string name, Guid commandId) : base(id)
    {
        Name = name;
        Emit(new TheAggregateIsCreated(name, commandId, Id));
    }

    public string Name { get; }

    public TheAggregate AddFriend(string friendName, Guid sourceCommandId) =>
        Emit(new TheAggregateHasChanged(friendName, sourceCommandId, Id));

    public TheAggregate AddFriendWithDifferentAggregateId(string friendName, Guid sourceCommandId) =>
        Emit(new TheAggregateHasChanged(friendName, sourceCommandId, Guid.NewGuid()));

    // ReSharper disable once UnusedMember.Global
    internal TheAggregate Execute(AChangeCommandWithoutHandler changeCommand) =>
        Emit(new TheAggregateHasChanged(changeCommand.Name, changeCommand.Id, Id));

    // ReSharper disable once UnusedMember.Global
    internal TheAggregate Execute(ASecondChangeCommandWithoutHandler changeCommand) =>
        Emit(new TheAggregateHasChanged(changeCommand.Name, changeCommand.Id, Id));
}