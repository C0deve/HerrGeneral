using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

/// <summary>
/// Projection item for AnotherThing
/// </summary>
public record AnotherThingProjectionItem(
    Guid Id,
    string Name,
    Guid ParentAggregateId
);

/// <summary>
/// Projection that tracks all created AnotherThing aggregates
/// </summary>
public class AnotherThingProjection : Projection<AnotherThingProjectionItem>,
    HerrGeneral.ReadSide.IProjectionEventHandler<AnotherThingCreated>
{
    /// <summary>
    /// Handles AnotherThingCreated event by adding to projection
    /// </summary>
    /// <param name="notification">The event notification</param>
    public void Handle(AnotherThingCreated notification) =>
        Add(new AnotherThingProjectionItem(
            notification.AggregateId,
            notification.Name,
            notification.ParentAggregateId
        ));

    /// <summary>
    /// Gets all AnotherThing IDs for a given parent aggregate
    /// </summary>
    /// <param name="parentAggregateId">The parent aggregate ID</param>
    /// <returns>Collection of AnotherThing IDs</returns>
    public IEnumerable<Guid> GetByParentId(Guid parentAggregateId) =>
        Find(item => item.ParentAggregateId == parentAggregateId)
             .Select(item => item.Id);
}
