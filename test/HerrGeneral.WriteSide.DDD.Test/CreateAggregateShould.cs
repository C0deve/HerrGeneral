using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;
using Xunit.Abstractions;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class CreateAggregateShould
{
    private readonly Mediator _mediator;
    private readonly Container _container;

    public CreateAggregateShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.AddSingleton<FriendAddedCounter>();
            cfg
                .UseHerrGeneral(configuration =>
                    configuration
                        .UseReadSideAssembly(typeof(Person).Assembly, typeof(Friends).Namespace!)
                )
                .RegisterDDDHandlers(typeof(Person).Assembly);
        });
        
        _mediator = _container.GetInstance<Mediator>();
    }

    [Fact]
    public async Task Create() =>
        await new CreatePerson("John", "Alfred").SendFromMediator(_mediator);


    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        await new CreatePerson("John", "Alfred").SendFromMediator(_mediator);

        _container.GetInstance<FriendAddedCounter>()
            .Value
            .ShouldBe(1);
    }
    
    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        await new CreatePerson("John", "Alfred").SendFromMediator(_mediator);

        _container.GetInstance<Friends>()
            .Names()
            .ShouldBe(["Alfred"]);
    }
}