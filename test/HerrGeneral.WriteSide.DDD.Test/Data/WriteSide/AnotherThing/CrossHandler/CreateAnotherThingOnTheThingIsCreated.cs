using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Event;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing.CrossHandler;

/// <summary>
/// Cross-aggregate handler that creates AnotherThing when TheAggregate is created
/// </summary>
public class CreateAnotherThingOnTheThingIsCreated : IDomainEventHandler<TheThingIsCreated, AnotherThing>
{
    /// <summary>
    /// Handles TheAggregateIsCreated event by creating a corresponding AnotherThing
    /// </summary>
    /// <param name="domainEvent">The domain event</param>
    /// <returns>Commands to execute</returns>
    public IEnumerable<AnotherThing> Handle(TheThingIsCreated domainEvent)
    {
        yield return new AnotherThing(Guid.NewGuid(),
            $"Related to {domainEvent.Name}",
            domainEvent.AggregateId,
            domainEvent.SourceCommandId);
    }
}
