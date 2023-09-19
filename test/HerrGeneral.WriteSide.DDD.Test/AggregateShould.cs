using HerrGeneral.Contracts;
using HerrGeneral.WriteSide.DDD.Exception;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Utils;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class AggregateShould
{
    [Fact]
    public void AddNewEventOnEmit()
    {
        var person = new Person(Guid.NewGuid(), "John")
            .AddFriend("Smith", Guid.NewGuid())
            .AddFriend("Adams", Guid.NewGuid());

        person.NewEvents.Count().ShouldBe(2);
    }

    [Fact]
    public void ClearNewEvents()
    {
        var person = new Person(Guid.NewGuid(), "John")
            .AddFriend("Smith", Guid.NewGuid())
            .AddFriend("Adams", Guid.NewGuid());

        person
            .ClearNewEvents()
            .NewEvents
            .Count()
            .ShouldBe(0);
    }

    [Fact]
    public void ThrowIfAnEventIsEmitWithADifferentAggregateId() =>
        Should.Throw<IdMismatchOnEventEmit<Person>>(() =>
            new Person(Guid.NewGuid(), "John").AddFriendWithDifferentAggregateId("Smith", Guid.NewGuid())
        );

    [Fact]
    public async Task ClearNewEventsOnSaveAndDispatch()
    {
        var person = new Person(Guid.NewGuid(), "John").AddFriend("Smith", Guid.NewGuid());

        await person.SaveAndDispatch(
            Guid.NewGuid(),
            (_, _) => Task.CompletedTask,
            new PersonRepository()
        );

        person.NewEvents.Count().ShouldBe(0);
    }

    [Fact]
    public async Task DispatchNewEventsOnSaveAndDispatch()
    {
        var dispatchedEvents = new List<IEvent>();

        await new Person(Guid.NewGuid(), "John")
            .AddFriend("Smith", Guid.NewGuid())
            .SaveAndDispatch(
                Guid.NewGuid(),
                (evt, _) =>
                {
                    dispatchedEvents.Add(evt);
                    return Task.CompletedTask;
                },
                new PersonRepository()
            );

        dispatchedEvents.Count.ShouldBe(1);
    }
    
    [Fact]
    public async Task SaveAggregateOnSaveAndDispatch()
    {
        var aggregateRepository = new PersonRepository();
        
        await new Person(Guid.NewGuid(), "John")
            .SaveAndDispatch(
                Guid.NewGuid(),
                (_, _) => Task.CompletedTask,
                aggregateRepository
            );

        aggregateRepository.HasSaved.ShouldBe(true);
    }
}