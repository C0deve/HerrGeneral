namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class FriendAddedHandler(FriendAddedCounter counter) : IEventHandler<FriendAdded>
{
    public void Handle(FriendAdded notification) => counter.Increment();
    
}

public class FriendAddedCounter
{
    public void Increment() => Value ++;

    public int Value { get; private set; }
}