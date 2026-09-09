using HerrGeneral.DDD;
using HerrGeneral.Test;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class ChangeAggregateShould
{
    private readonly Mediator _mediator;
    private readonly IServiceProvider _container;

    public ChangeAggregateShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IAggregateRepository<TheThing>, Repository<TheThing>>()
            .AddSingleton<IAggregateRepository<AnotherThing>, Repository<AnotherThing>>()
            .AddSingleton<ChangesCounter>()
            .AddSingleton<AProjection>()
            .AddSingleton<AnotherThingProjection>()
            .AddSingleton<TheThingTracker>()
            .AddSingleton<ToBeNotifiedOnNameChangedTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(TheThing).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide")
                    .ScanReadSideOn(typeof(AProjection).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.ReadModel")
            );

        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Change() =>
        await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThing("John"), _mediator)
            .Then(personId =>
                HerrGeneral.DDD.Extensions.SendFrom(new ChangeTheThing("Adams", personId), _mediator))
            .ShouldSuccess();

    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThing("John"), _mediator)
            .Then(personId =>
                HerrGeneral.DDD.Extensions.SendFrom(new ChangeTheThing("Adams", personId), _mediator))
            .ShouldSuccess();

        _container.GetRequiredService<ChangesCounter>()
            .Count
            .ShouldBe(2);
    }

    [Fact]
    public async Task ChangeDifferentAggregatesConcurrently()
    {
        var create1 = await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThing("Alice"), _mediator);
        var create2 = await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThing("Bob"), _mediator);

        var id1 = create1.Match(id => id, _ => Guid.Empty, _ => Guid.Empty);
        var id2 = create2.Match(id => id, _ => Guid.Empty, _ => Guid.Empty);

        var task1 = Task.Run(() => HerrGeneral.DDD.Extensions.SendFrom(new ChangeTheThing("Alice-Updated", id1), _mediator));
        var task2 = Task.Run(() => HerrGeneral.DDD.Extensions.SendFrom(new ChangeTheThing("Bob-Updated", id2), _mediator));

        var results = await Task.WhenAll(task1, task2);
        results[0].IsSuccess.ShouldBeTrue();
        results[1].IsSuccess.ShouldBeTrue();

        var projection = _container.GetRequiredService<AProjection>();
        projection.All().Select(x => x.Name).ShouldBe(["Alice-Updated", "Bob-Updated"], ignoreOrder: true);
    }
}