namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;

public class ToBeNotifiedOnNameChangedTracker 
{
    private readonly List<Guid> _ids = [];

    public Guid[] GetIds()=> [.. _ids];

    public void Track(Guid id) => _ids.Add(id);

}