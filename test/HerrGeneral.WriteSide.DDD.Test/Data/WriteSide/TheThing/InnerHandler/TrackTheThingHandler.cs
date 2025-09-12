using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class TrackTheThingHandler(TheThingTracker tracker) :
    IVoidDomainEventHandler<TheThingIsCreated>,
    IVoidDomainEventHandler<TheThingHasChanged>
{
    public void Handle(TheThingIsCreated notification) => 
        tracker.Track(notification.AggregateId);

    public void Handle(TheThingHasChanged notification) => 
        tracker.UnTrack(notification.AggregateId);
}