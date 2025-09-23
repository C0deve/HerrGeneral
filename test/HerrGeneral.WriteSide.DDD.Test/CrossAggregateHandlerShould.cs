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

public class CrossAggregateHandlerShould
{
    private readonly Mediator _mediator;
    private readonly IServiceProvider _container;

    public CrossAggregateHandlerShould(ITestOutputHelper output)
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
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreateTheThing("John")
            .SendFrom<Guid>(_mediator)
            .Then(id =>
                new ChangeTheThing("Adams", id)
                    .SendFrom(_mediator))
            .ShouldSuccess();

        _container
            .GetRequiredService<ToBeNotifiedOnNameChangedTracker>()
            .GetIds()
            .Select(_container.GetRequiredService<IAggregateRepository<AnotherThing>>().Get)
            .ShouldHaveSingleItem()
            .IsParentNameLiked
            .ShouldBeTrue();
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await
            new CreateTheThing("John")
                .SendFrom<Guid>(_mediator)
                .Then(id =>
                    new ChangeTheThing("Adams", id).SendFrom(_mediator))
                .ShouldSuccess();

        _container.GetRequiredService<AnotherThingProjection>()
            .All()
            .ShouldHaveSingleItem()
            .IsParentNameLiked
            .ShouldBeTrue();
    }
}