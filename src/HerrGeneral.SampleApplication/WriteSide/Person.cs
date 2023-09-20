using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public sealed class Person : Aggregate<Person>
{
    public string Name { get; }
    public string MyFriend { get; private set; }
    
    internal Person(CreatePerson createPerson, Guid id) : base(id)
    {
        Name = createPerson.Name;
        MyFriend = createPerson.MyFriend;
        EmitFriendChanged(createPerson.Id);
    }
    
    // ReSharper disable once UnusedMember.Global
    internal Person Execute(SetFriend command)
    {
        MyFriend = command.Friend;
        return EmitFriendChanged(command.Id);
    }
    
    private Person EmitFriendChanged(Guid sourceCommandId) => 
        Emit(new FriendChanged(Name, MyFriend, sourceCommandId, Id));
}