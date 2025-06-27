namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public class Friends(List<string> friendNames) : HerrGeneral.ReadSide.IEventHandler<FriendAdded>
{
    
    public void Handle(FriendAdded notification) => friendNames.Add(notification.Name);
    
    public IReadOnlyCollection<string> Names() => friendNames.AsReadOnly();
}