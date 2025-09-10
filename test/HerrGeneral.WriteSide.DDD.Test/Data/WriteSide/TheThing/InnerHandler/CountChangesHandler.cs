using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class CountChangesHandler(ChangesCounter counter) :
    IEventHandler<TheThingIsCreated>,
    IEventHandler<TheThingHasChanged>
{
    public IEnumerable<object> Handle(TheThingIsCreated notification)
    {
        counter.Increment();
        return [];
    }
    
    public IEnumerable<object> Handle(TheThingHasChanged notification)
    {
         counter.Increment();
         return [];
    }

    
}