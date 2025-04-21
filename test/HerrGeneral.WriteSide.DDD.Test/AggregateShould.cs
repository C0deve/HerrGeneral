using HerrGeneral.WriteSide.DDD.Exception;
using HerrGeneral.WriteSide.DDD.Test.Data;
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
}