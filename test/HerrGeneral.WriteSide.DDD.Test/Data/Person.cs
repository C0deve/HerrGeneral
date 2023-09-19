namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class Person : Aggregate<Person>
{
    public string Name { get; }
    
    public Person(Guid id, string name) : base(id) => 
        Name = name;

    public Person(ACreateCommandWithoutHandler command, Guid id) : this(id, command.Name)
    {
    }
    
    public Person AddFriend(string friendName, Guid sourceCommandId) => 
        Emit(new FriendAdded(friendName, sourceCommandId, Id));

    public Person AddFriendWithDifferentAggregateId(string friendName, Guid sourceCommandId) => 
        Emit(new FriendAdded(friendName, sourceCommandId, Guid.NewGuid()));

    // ReSharper disable once UnusedMember.Global
    public Person Execute(AChangeCommandWithoutHandler changeCommand) =>
        Emit(new FriendAdded(changeCommand.Name, changeCommand.Id, Id));
}