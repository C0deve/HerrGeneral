namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public class Friends : ProjectionBase<string>, HerrGeneral.ReadSide.IEventHandler<FriendAdded>
{
    public void Handle(FriendAdded notification) => 
        Data.Add(notification.Name);
}