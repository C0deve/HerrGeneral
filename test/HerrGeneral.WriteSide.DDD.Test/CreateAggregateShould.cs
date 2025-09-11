using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class CreateAggregateShould
{
    private readonly Mediator _mediator;
    private readonly IServiceProvider _container;

    public CreateAggregateShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IAggregateRepository<TheThing>, Repository<TheThing>>()
            .AddSingleton<IAggregateRepository<AnotherThing>, Repository<AnotherThing>>()
            .AddSingleton<ChangesCounter>()
            .AddSingleton<AProjection>()
            .AddSingleton<AnotherThingProjection>()
            .AddSingleton<TheThingTracker>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(TheThing).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide")
                    .ScanReadSideOn(typeof(TheThing).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.ReadModel")
            );
        
        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Create() =>
        await new CreateTheThing("John")
            .SendFrom(_mediator)
            .ShouldSuccess();


    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreateTheThing("John")
            .SendFrom(_mediator)
            .ShouldSuccess();

        _container.GetRequiredService<ChangesCounter>()
            .Count
            .ShouldBe(1);
    }
    
    [Fact]
    public async Task DispatchEventsCreatedInWriteSideEventHandler()
    {
        await new CreateTheThing("John")
            .SendFrom(_mediator)
            .ShouldSuccess();

        _container.GetRequiredService<AnotherThingProjection>()
            .All()
            .Select(x => x.Name)
            .ShouldBe(["Related to John"]);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreateTheThing("John")
            .SendFrom(_mediator)
            .ShouldSuccess();

        _container.GetRequiredService<AProjection>()
            .All()
            .Select(x => x.Name)
            .ShouldBe(["John"]);
    }
}