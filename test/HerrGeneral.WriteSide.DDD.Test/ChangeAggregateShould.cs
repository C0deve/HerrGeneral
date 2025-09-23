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
    public async Task DispatchEventsOnReadSide()
    {
        await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThing("John"), _mediator)
            .Then(personId =>
                HerrGeneral.DDD.Extensions.SendFrom(new ChangeTheThing("Adams", personId), _mediator))
            .ShouldSuccess();

        _container.GetRequiredService<AProjection>()
            .All()
            .Select(x => x.Name)
            .ShouldBe(["Adams"]);
    }
    
    
}