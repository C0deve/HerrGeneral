using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public sealed class Person : Aggregate<Person>
{
    public string Name { get; }
    public string MyFriend { get; private set; }
    
    public Person(Guid id, string name, string myFriend, Guid sourceCommandId) : base(id)
    {
        Name = name;
        MyFriend = myFriend;
        EmitFriendChanged(sourceCommandId);
    }
    
    public Person SetFriend(string friendName, Guid sourceCommandId)
    {
        MyFriend = friendName;
        return EmitFriendChanged(sourceCommandId);
    }
    
    private Person EmitFriendChanged(Guid sourceCommandId) => 
        Emit(new FriendChanged(Name, MyFriend, sourceCommandId, Id));
}