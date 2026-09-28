namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;

public class TheThingTracker
{
    private readonly List<Guid> _allList = [];

    public void Track(Guid theThingId)
    {
        lock (_allList)
        {
            _allList.Add(theThingId);
        }
    }

    public void UnTrack(Guid theThingId)
    {
        lock (_allList)
        {
            _allList.Remove(theThingId);
        }
    }

    public IEnumerable<Guid> All()
    {
        lock (_allList)
        {
            return [.. _allList];
        }
    }
}