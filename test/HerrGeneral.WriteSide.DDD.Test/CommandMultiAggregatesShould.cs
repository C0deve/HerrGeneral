using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.InnerHandler;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class CommandMultiAggregatesShould
{
    private readonly Mediator _mediator;
    private readonly IServiceProvider _container;

    public CommandMultiAggregatesShould(ITestOutputHelper output)
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
                    .ScanReadSideOn(typeof(TheThing).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.ReadModel")
            )
            .RegisterDDDHandlers(typeof(TheThing).Assembly);

        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Success()
    {
        await new CreateTheThing("John").AssertSendFrom<Guid>(_mediator);
        await new CreateTheThing("Doe").AssertSendFrom<Guid>(_mediator);

        await new DeleteAllTheThings().SendFrom(_mediator).ShouldSuccess();
        
        var repository = _container.GetRequiredService<IAggregateRepository<TheThing>>();
        _container.GetRequiredService<TheThingTracker>()
            .All()
            .Select(repository.Get)
            .Select(list => list.ShouldNotBeNull().IsDeleted)
            .ShouldBe([true, true]);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreateTheThing("John").AssertSendFrom<Guid>(_mediator);
        await new CreateTheThing("Doe").AssertSendFrom<Guid>(_mediator);

        await new DeleteAllTheThings().AssertSendFrom(_mediator);
        
        _container.GetRequiredService<AProjection>()
            .All()
            .Select(item => item.IsDeleted)
            .ShouldBe([true, true]);
    }
}