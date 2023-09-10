namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class Person : Aggregate<Person>
{
    public Person AddFriend(string friendName, Guid sourceCommandId) => 
        Emit(new FriendAdded(friendName, sourceCommandId, Id));

    public Person AddFriendWithDifferentAggregateId(string friendName, Guid sourceCommandId) => 
        Emit(new FriendAdded(friendName, sourceCommandId, Guid.NewGuid()));

    public Person(Guid id) : base(id)
    {
    }
}