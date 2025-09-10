using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public record AProjectionItem(Guid TheAggregateId, string Name, bool IsDeleted = false);

public class AProjection : Projection<AProjectionItem>,
    HerrGeneral.ReadSide.IProjectionEventHandler<TheThingIsCreated>,
    HerrGeneral.ReadSide.IProjectionEventHandler<TheThingHasChanged>,
    HerrGeneral.ReadSide.IProjectionEventHandler<TheThingDeleted>
{
    public void Handle(TheThingIsCreated notification) =>
        Add(new AProjectionItem(notification.AggregateId, notification.Name));

    public void Handle(TheThingHasChanged notification) =>
        Update(
            item => item.TheAggregateId == notification.AggregateId,
            item => item with { Name = notification.Name }
        );

    public void Handle(TheThingDeleted notification) =>
        Update(
            item => item.TheAggregateId == notification.AggregateId,
            item => item with { IsDeleted = true }
        );
}