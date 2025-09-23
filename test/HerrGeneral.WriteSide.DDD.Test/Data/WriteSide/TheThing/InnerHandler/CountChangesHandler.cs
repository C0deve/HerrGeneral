using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;

public class CountChangesHandler(ChangesCounter counter) :
    IVoidDomainEventHandler<TheThingIsCreated>,
    IVoidDomainEventHandler<TheThingNameChanged>
{
    public void Handle(TheThingIsCreated notification) => counter.Increment();

    public void Handle(TheThingNameChanged notification) => counter.Increment();
}