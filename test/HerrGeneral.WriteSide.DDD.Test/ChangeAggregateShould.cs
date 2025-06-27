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
using Xunit.Sdk;

namespace HerrGeneral.WriteSide.DDD.Test;

public class ChangeAggregateShould
{
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
    }

    [Fact]
    public async Task Change()
    {
        var personId = await new CreatePerson("John", "Alfred").Send(_container);
        await new AddFriend("Adams", personId).Send(_container);
    }

    [Fact]
    public async Task DispatchEventsOnWriteSide()
    {
        var personId = await new CreatePerson("John", "Alfred").Send<Guid>(_container);
        await new AddFriend("Adams", personId).Send(_container);

        _container.GetInstance<FriendAddedCounter>()
            .Value
            .ShouldBe(2);
    }
    
    [Fact]
    public async Task DispatchEventsOnReadSide()
    {
        var personId = await new CreatePerson("John", "Alfred").Send<Guid>(_container);
        await new AddFriend("Adams", personId).Send(_container);

        _container.GetInstance<Friends>()
            .Names()
            .ShouldBe(["Alfred", "Adams"]);
    }
}

public static class Ext
{
    public static async Task<Guid> Send<T>(this Create<T> request, IServiceProvider serviceProvider)
        where T : IAggregate =>
        (await serviceProvider
            .GetRequiredService<Mediator>()
            .Send<Guid>(request))
        .Match(id => id,
            domainError => throw new XunitException($"Command have a domain error of type<{domainError.GetType()}>. {domainError}"),
            exception => throw new XunitException($"Command have a panic exception of type<{exception.GetType()}>. {exception.Message}", exception));
}