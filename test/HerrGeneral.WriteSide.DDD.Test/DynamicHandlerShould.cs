using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
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
            .AddSingleton<IAggregateRepository<TheAggregate>, Repository<TheAggregate>>()
            .AddSingleton<IAggregateFactory<TheAggregate>, DefaultAggregateFactory<TheAggregate>>()
            .AddHerrGeneral(configuration => configuration)
            .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly);

        _mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();
}

[Fact]
public async Task HandleAChangeCommandWithoutHandler() =>
    await new CreateTheAggregateNoHandler("John", "Alfred")
        .SendFrom(_mediator)
        .Then(personId =>
            new AChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator));

[Fact]
public async Task HandleASecondChangeCommandWithoutHandler() =>
    await new CreateTheAggregateNoHandler("John", "Alfred")
        .SendFrom(_mediator)
        .Then(personId =>
            new ASecondChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator));


[Fact]
public async Task HandleACreateCommandWithoutHandler() =>
    await new CreateTheAggregateNoHandler("John", "Alfred").SendFrom(_mediator);

[Fact]
public async Task ThrowIfExecuteMethodNotFound() =>
    await new CreateTheAggregateNoHandler("John", "Alfred")
        .SendFrom(_mediator)
        .Then(personId =>
            new AThirdChangeCommandWithoutHandler("Remy", personId).SendFrom(_mediator))
        .ShouldFailWithPanicExceptionOfType<MissingMethodException>();


[Fact]
public async Task ThrowIfConstructorNotFound() =>
    await new CreateTheAggregateNoHandlerWithFailure()
        .SendFrom(_mediator)
        .ShouldFailWithPanicExceptionOfType<MissingMethodException, Guid>();

}