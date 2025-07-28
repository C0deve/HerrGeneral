namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public class Friends : HerrGeneral.ReadSide.IEventHandler<FriendAdded>
{
    private readonly List<string> _friendNames = [];
    public IReadOnlyCollection<string> Names() => _friendNames.AsReadOnly();
    
    public void Handle(FriendAdded notification) => _friendNames.Add(notification.Name);
}