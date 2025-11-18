using HerrGeneral.DDD;
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
            .AddSingleton<ToBeNotifiedOnNameChangedTracker>()
            .AddHerrGeneral(configuration => configuration
                .ScanWriteSideOn(typeof(AChangeCommandWithoutHandler).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide"));

        _mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();
}

[Fact]
public async Task HandleAChangeCommandWithoutHandler() =>
    await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThingNoHandler("John"), _mediator)
        .Then(personId =>
            HerrGeneral.DDD.Extensions.SendFrom(new AChangeCommandWithoutHandler("Remy", personId), _mediator))
        .ShouldSuccess();

[Fact]
public async Task HandleASecondChangeCommandWithoutHandler() =>
    await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThingNoHandler("John"), _mediator)
        .Then(personId =>
            HerrGeneral.DDD.Extensions.SendFrom(new ASecondChangeCommandWithoutHandler("Remy", personId), _mediator))
    .ShouldSuccess();


[Fact]
public async Task HandleACreateCommandWithoutHandler() =>
    await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThingNoHandler("John"), _mediator)
        .ShouldSuccess();

[Fact]
public async Task ThrowIfExecuteMethodNotFound() =>
    await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThingNoHandler("John"), _mediator)
        .Then(personId =>
            HerrGeneral.DDD.Extensions.SendFrom(new AThirdChangeCommandWithoutHandler("Remy", personId), _mediator))
        .ShouldFailWithPanicExceptionOfType<MissingMethodException>();


[Fact]
public async Task ThrowIfConstructorNotFound() =>
    await HerrGeneral.DDD.Extensions.SendFrom(new CreateTheThingNoHandlerWithFailure(), _mediator)
        .ShouldFailWithPanicExceptionOfType<Guid, MissingMethodException>();

}