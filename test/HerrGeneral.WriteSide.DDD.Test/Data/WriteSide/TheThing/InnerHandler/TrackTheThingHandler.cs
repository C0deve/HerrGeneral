using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class TrackTheThingHandler(TheThingTracker tracker) :
    IEventHandler<TheThingIsCreated>,
    IEventHandler<TheThingHasChanged>
{
    public IEnumerable<object> Handle(TheThingIsCreated notification)
    {
        tracker.Track(notification.AggregateId);
        return [];
    }

    public IEnumerable<object> Handle(TheThingHasChanged notification)
    {
        tracker.UnTrack(notification.AggregateId);
        return [];
    }
}