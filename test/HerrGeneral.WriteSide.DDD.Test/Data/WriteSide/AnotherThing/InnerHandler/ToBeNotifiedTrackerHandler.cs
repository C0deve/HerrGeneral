using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.InnerHandler;

public class ToBeNotifiedTrackerHandler(ToBeNotifiedOnNameChangedTracker tracker) 
    : IVoidDomainEventHandler<SubscribedToNameChangeNotifications>
{
    public void Handle(SubscribedToNameChangeNotifications notification) => 
        tracker.Track(notification.AggregateId);
}
