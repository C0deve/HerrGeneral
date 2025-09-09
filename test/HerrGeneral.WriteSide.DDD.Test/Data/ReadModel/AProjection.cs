using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public record AProjectionItem(Guid TheAggregateId, string Name);

public class AProjection : Projection<AProjectionItem>,
    HerrGeneral.ReadSide.IEventHandler<TheAggregateIsCreated>,
    HerrGeneral.ReadSide.IEventHandler<TheAggregateHasChanged>
{
    public void Handle(TheAggregateIsCreated notification) =>
        Add(new AProjectionItem(notification.AggregateId, notification.Name));

    public void Handle(TheAggregateHasChanged notification) =>
        Update(
            item => item.TheAggregateId == notification.AggregateId,
            item => item with { Name = notification.Name }
        );
}