using HerrGeneral.DDD.Exception;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class AggregateShould
{
    [Fact]
    public void AddNewEventOnEmit()
    {
        var person = new TheThing(Guid.NewGuid(), "John", Guid.NewGuid())
            .AddFriend("Smith", Guid.NewGuid())
            .AddFriend("Adams", Guid.NewGuid());

        person.NewEvents.Count().ShouldBe(3);
    }

    [Fact]
    public void ClearNewEvents()
    {
        var person = new TheThing(Guid.NewGuid(), "John", Guid.NewGuid())
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
        Should.Throw<IdMismatchOnEventEmit<TheThing>>(() =>
            new TheThing(Guid.NewGuid(), "John", Guid.NewGuid()).AddFriendWithDifferentAggregateId("Smith", Guid.NewGuid())
        );

    [Fact]
    public void ExposeTypedIdInAggregateNotFoundException()
    {
        var exString = new AggregateNotFound<TheThing, string>("ORDER-123");
        exString.Id.ShouldBe("ORDER-123");
        exString.Message.ShouldContain("ORDER-123");

        var guid = Guid.NewGuid();
        var exGuid = new AggregateNotFound<TheThing>(guid);
        exGuid.Id.ShouldBe(guid);
        exGuid.Message.ShouldContain(guid.ToString());
    }
}