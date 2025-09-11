using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class CountChangesHandler(ChangesCounter counter) :
    IVoidDomainEventHandler<TheThingIsCreated>,
    IVoidDomainEventHandler<TheThingHasChanged>
{
    public void Handle(TheThingIsCreated notification) => counter.Increment();

    public void Handle(TheThingHasChanged notification) => counter.Increment();
}