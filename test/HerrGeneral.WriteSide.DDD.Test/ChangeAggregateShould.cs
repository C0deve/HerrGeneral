using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
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
            .AddSingleton<IAggregateRepository<Person>, PersonRepository>()
            .AddSingleton<FriendAddedCounter>()
            .UseHerrGeneral(configuration =>
                configuration
                    .UseReadSideAssembly(typeof(Person).Assembly, typeof(Friends).Namespace!)
            )
            .RegisterDDDHandlers(typeof(Person).Assembly);
        
        _container = services.BuildServiceProvider();

        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Change() =>
        await new CreatePerson("John", "Alfred")
            .SendFrom(_mediator)
            .Then(personId =>
                new AddFriend("Adams", personId).SendFrom(_mediator))
            .ShouldSuccess();

    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreatePerson("John", "Alfred")
            .SendFrom(_mediator)
            .Then(personId =>
                new AddFriend("Adams", personId).SendFrom(_mediator));

        _container.GetRequiredService<FriendAddedCounter>()
            .Value
            .ShouldBe(2);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreatePerson("John", "Alfred")
            .SendFrom(_mediator)
            .Then(personId =>
                new AddFriend("Adams", personId).SendFrom(_mediator));

        _container.GetRequiredService<Friends>()
            .Names()
            .ShouldBe(["Alfred", "Adams"]);
    }
}