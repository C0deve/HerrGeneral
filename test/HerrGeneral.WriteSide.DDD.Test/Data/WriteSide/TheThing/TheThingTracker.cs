namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;

public class TheThingTracker
{
    private readonly List<Guid> _allList =  [];

    public void Track(Guid theThingId) =>
        _allList.Add(theThingId);
    
    public void UnTrack(Guid theThingId) => 
        _allList.Remove(theThingId);
    
    public IEnumerable<Guid> All() => _allList.AsReadOnly();
}