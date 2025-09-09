namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;

public class ChangesCounter
{
    public void Increment() => Count ++;

    public int Count { get; private set; }
}