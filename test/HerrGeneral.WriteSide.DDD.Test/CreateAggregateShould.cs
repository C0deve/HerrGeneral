using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
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
            .AddSingleton<IAggregateRepository<Person>, PersonRepository>()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<FriendAddedCounter>(_ => new FriendAddedCounter())
            .UseHerrGeneral(configuration =>
                configuration
                    .UseReadSideAssembly(typeof(Person).Assembly, typeof(Friends).Namespace!)
            )
            .RegisterDDDHandlers(typeof(Person).Assembly);

        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Create() =>
        await new CreatePerson("John", "Alfred").SendFrom(_mediator);


    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreatePerson("John", "Alfred").SendFrom(_mediator);

        _container.GetRequiredService<FriendAddedCounter>()
            .Value
            .ShouldBe(1);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreatePerson("John", "Alfred").SendFrom(_mediator);

        _container.GetRequiredService<Friends>()
            .Names()
            .ShouldBe(["Alfred"]);
    }
}