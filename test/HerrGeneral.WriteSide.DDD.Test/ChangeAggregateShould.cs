using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class ChangeAggregateShould
{
    private readonly Mediator _mediator;
    private readonly Container _container;

    public ChangeAggregateShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.AddSingleton<FriendAddedCounter>();
            cfg.UseHerrGeneral(configuration =>
                    configuration
                        .UseReadSideAssembly(typeof(Person).Assembly, typeof(Friends).Namespace!)
                )
                .RegisterDDDHandlers(typeof(Person).Assembly);
        });

        _mediator = _container.GetInstance<Mediator>();
    }

    [Fact]
    public async Task Change() =>
        await new CreatePerson("John", "Alfred")
            .SendFromMediator(_mediator)
            .Then(personId =>
                new AddFriend("Adams", personId).SendFromMediator(_mediator))
            .ShouldSuccess();

    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreatePerson("John", "Alfred")
            .SendFromMediator(_mediator)
            .Then(personId => 
                new AddFriend("Adams", personId).SendFromMediator(_mediator));

        _container.GetInstance<FriendAddedCounter>()
            .Value
            .ShouldBe(2);
    }

    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreatePerson("John", "Alfred")
            .SendFromMediator(_mediator)
            .Then(personId => 
                new AddFriend("Adams", personId).SendFromMediator(_mediator));

        _container.GetInstance<Friends>()
            .Names()
            .ShouldBe(["Alfred", "Adams"]);
    }
}