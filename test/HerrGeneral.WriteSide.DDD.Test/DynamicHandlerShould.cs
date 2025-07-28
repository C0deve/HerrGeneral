using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
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
            .AddSingleton<IAggregateRepository<Person>, PersonRepository>()
            .AddSingleton<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>()
            .UseHerrGeneral(configuration => configuration)
            .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly);

        _mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();
}

[Fact]
public async Task HandleAChangeCommandWithoutHandler() =>
    await new ACreateCommandWithoutHandler("John", "Alfred")
        .SendFromMediator(_mediator)
        .Then(personId =>
            new AChangeCommandWithoutHandler("Remy", personId).SendFromMediator(_mediator));

[Fact]
public async Task HandleASecondChangeCommandWithoutHandler() =>
    await new ACreateCommandWithoutHandler("John", "Alfred")
        .SendFromMediator(_mediator)
        .Then(personId =>
            new ASecondChangeCommandWithoutHandler("Remy", personId).SendFromMediator(_mediator));


[Fact]
public async Task HandleACreateCommandWithoutHandler() =>
    await new ACreateCommandWithoutHandler("John", "Alfred").SendFromMediator(_mediator);

[Fact]
public async Task ThrowIfExecuteMethodNotFound() =>
    await new ACreateCommandWithoutHandler("John", "Alfred")
        .SendFromMediator(_mediator)
        .Then(personId =>
            new AThirdChangeCommandWithoutHandler("Remy", personId).SendFromMediator(_mediator))
        .ShouldFailWithPanicExceptionOfType<MissingMethodException>();


[Fact]
public async Task ThrowIfConstructorNotFound() =>
    await new ASecondCreateCommandWithoutHandler()
        .SendFromMediator(_mediator)
        .ShouldFailWithPanicExceptionOfType<MissingMethodException, Guid>();

}