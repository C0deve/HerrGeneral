using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.CrossHandler;

public class ACrossAggregateChangeHandler(ToBeNotifiedOnNameChangedTracker service) : 
    ChangesPlanner<AnotherThing>, 
    ICrossAggregateChangeHandler<TheThingNameChanged, AnotherThing>
{
    public ChangeRequests<AnotherThing> Handle(TheThingNameChanged notification) =>
        Changes
            .Add(
                x => x.ReactToTheThingNameChanged(notification.AggregateId,
                    notification.Name,
                    notification.SourceCommandId),
                service.GetIds());
}