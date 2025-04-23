namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class Person(Guid id, string name) : Aggregate<Person>(id)
{
    public Person(ACreateCommandWithoutHandler command, Guid aggregateId) : this(aggregateId, command.Name)
    {
    }

    public string Name { get; } = name;

    public Person AddFriend(string friendName, Guid sourceCommandId) =>
        Emit(new FriendAdded(friendName, sourceCommandId, Id));

    public Person AddFriendWithDifferentAggregateId(string friendName, Guid sourceCommandId) =>
        Emit(new FriendAdded(friendName, sourceCommandId, Guid.NewGuid()));

    // ReSharper disable once UnusedMember.Global
    internal Person Execute(AChangeCommandWithoutHandler changeCommand) =>
        Emit(new FriendAdded(changeCommand.Name, changeCommand.Id, Id));

    // ReSharper disable once UnusedMember.Global
    internal Person Execute(ASecondChangeCommandWithoutHandler changeCommand) =>
        Emit(new FriendAdded(changeCommand.Name, changeCommand.Id, Id));
}