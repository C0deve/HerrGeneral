namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;

public class ToBeNotifiedOnNameChangedTracker 
{
    private readonly List<Guid> _ids = [];

    public Guid[] GetIds()
    {
        lock (_ids)
        {
            return [.. _ids];
        }
    }

    public void Track(Guid id)
    {
        lock (_ids)
        {
            _ids.Add(id);
        }
    }
}