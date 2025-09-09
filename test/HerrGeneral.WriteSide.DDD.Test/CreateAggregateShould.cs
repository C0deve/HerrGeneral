using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
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
            .AddSingleton<IAggregateRepository<TheAggregate>, Repository<TheAggregate>>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<ChangesCounter>()
            .AddSingleton<AProjection>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanReadSideOn(typeof(TheAggregate).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.ReadModel")
            )
            .RegisterDDDHandlers(typeof(TheAggregate).Assembly);

        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Create() =>
        await new CreateTheAggregate("John").SendFrom(_mediator);


    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreateTheAggregate("John").SendFrom(_mediator);

        _container.GetRequiredService<ChangesCounter>()
            .Count
            .ShouldBe(1);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreateTheAggregate("John").SendFrom(_mediator);

        _container.GetRequiredService<AProjection>()
            .All()
            .Select(x => x.Name)
            .ShouldBe(["John"]);
    }
}