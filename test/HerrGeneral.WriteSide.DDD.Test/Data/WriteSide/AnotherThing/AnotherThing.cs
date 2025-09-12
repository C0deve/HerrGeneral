using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;

/// <summary>
/// Another aggregate that is created when TheAggregate is created
/// </summary>
public class AnotherThing : Aggregate<AnotherThing>
{
    public AnotherThing(Guid id, string name, Guid parentAggregateId, Guid commandId) : base(id)
    {
        Name = name;
        ParentAggregateId = parentAggregateId;

        Emit(new AnotherThingCreated(name, parentAggregateId, commandId, Id));
    }

    public string Name { get; }
    public Guid ParentAggregateId { get; }
}
