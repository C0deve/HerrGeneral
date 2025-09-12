using HerrGeneral.Core.DDD;
using HerrGeneral.Test;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.AnotherThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class DynamicHandlerShould
{
    private readonly Mediator _mediator;

    public DynamicHandlerShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IAggregateRepository<TheThing>, Repository<TheThing>>()
            .AddSingleton<IAggregateFactory<TheThing>, DefaultAggregateFactory<TheThing>>()
            .AddSingleton<IAggregateRepository<AnotherThing>, Repository<AnotherThing>>()
            .AddSingleton<ChangesCounter>()
            .AddSingleton<TheThingTracker>()
            .AddSingleton<AProjection>()
            .AddSingleton<AnotherThingProjection>()
            .AddHerrGeneral(configuration => configuration
                .ScanWriteSideOn(typeof(AChangeCommandWithoutHandler).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide"));

        _mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();
}

[Fact]
public async Task HandleAChangeCommandWithoutHandler() =>
    await new CreateTheThingNoHandler("John")
        .SendFrom(_mediator)
        .Then(personId =>
            new AChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator))
        .ShouldSuccess();

[Fact]
public async Task HandleASecondChangeCommandWithoutHandler() =>
    await new CreateTheThingNoHandler("John")
        .SendFrom(_mediator)
        .Then(personId =>
            new ASecondChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator))
    .ShouldSuccess();


[Fact]
public async Task HandleACreateCommandWithoutHandler() =>
    await new CreateTheThingNoHandler("John")
        .SendFrom(_mediator)
        .ShouldSuccess();

[Fact]
public async Task ThrowIfExecuteMethodNotFound() =>
    await new CreateTheThingNoHandler("John")
        .SendFrom(_mediator)
        .Then(personId =>
            new AThirdChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator))
        .ShouldFailWithPanicExceptionOfType<MissingMethodException>();


[Fact]
public async Task ThrowIfConstructorNotFound() =>
    await new CreateTheThingNoHandlerWithFailure()
        .SendFrom(_mediator)
        .ShouldFailWithPanicExceptionOfType<MissingMethodException, Guid>();

}