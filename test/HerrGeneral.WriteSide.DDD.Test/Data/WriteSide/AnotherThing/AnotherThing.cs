using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;

/// <summary>
/// Another aggregate
/// </summary>
public class AnotherThing : Aggregate<AnotherThing>
{
    public string Name { get; }
    public Guid ParentAggregateId { get; }
    public bool IsParentNameLiked { get; private set; }

    public AnotherThing(Guid id, string name, Guid parentAggregateId, Guid commandId) : base(id)
    {
        Name = name;
        ParentAggregateId = parentAggregateId;

        Emit(new AnotherThingCreated(name, parentAggregateId, commandId, Id))
            .Emit(new SubscribedToNameChangeNotifications(commandId, Id));
    }

    public AnotherThing ReactToTheThingNameChanged(Guid parentId, string parentName, Guid commandId)
    {
        if (parentId != ParentAggregateId)
            return this;

        IsParentNameLiked = parentName.StartsWith("a", StringComparison.InvariantCultureIgnoreCase);

        return Emit(
            IsParentNameLiked
                ? new ParentNameLiked(commandId, Id)
                : new ParentNameNotLiked(commandId, Id)
        );
    }
}