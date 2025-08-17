namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class FriendAddedHandler(FriendAddedCounter counter) : IEventHandler<FriendAdded>
{
    public IEnumerable<object> Handle(FriendAdded notification)
    {
         counter.Increment();
         return [];
    }
}

public class FriendAddedCounter
{
    public void Increment() => Value ++;

    public int Value { get; private set; }
}