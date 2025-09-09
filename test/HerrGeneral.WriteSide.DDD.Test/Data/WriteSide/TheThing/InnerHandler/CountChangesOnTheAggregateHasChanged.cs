using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class CountChangesOnTheAggregateHasChanged(ChangesCounter counter) :
    IEventHandler<TheAggregateIsCreated>,
    IEventHandler<TheAggregateHasChanged>
{
    public IEnumerable<object> Handle(TheAggregateIsCreated notification)
    {
        counter.Increment();
        return [];
    }
    
    public IEnumerable<object> Handle(TheAggregateHasChanged notification)
    {
         counter.Increment();
         return [];
    }

    
}