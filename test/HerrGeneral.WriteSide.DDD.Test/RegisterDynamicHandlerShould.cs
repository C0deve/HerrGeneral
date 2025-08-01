using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class RegisterDynamicHandlersShould
{
    private readonly IServiceProvider _container;

    public RegisterDynamicHandlersShould(ITestOutputHelper output) =>
        _container = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IAggregateRepository<Person>, PersonRepository>()
            .AddSingleton<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>()
            .AddHerrGeneral(scanner => scanner)
            .RegisterDDDHandlers(typeof(Person).Assembly)
            .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly)
            .BuildServiceProvider();

    [Fact]
    public void RegisterDynamicHandlersForChangeAggregateCommandWithoutHandler() =>
        _container
            .GetServices<ICommandHandler<AChangeCommandWithoutHandler, Unit>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void NotRegisterDynamicHandlersForChangeAggregateCommandWithHandler() =>
        _container
            .GetServices<ICommandHandler<AddFriend, Unit>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void RegisterDynamicHandlersForCreateAggregateCommandWithoutHandler() =>
        _container
            .GetServices<ICommandHandler<ACreateCommandWithoutHandler, Guid>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void NotRegisterDynamicHandlersForCreateAggregateCommandWithHandler() =>
        _container
            .GetServices<ICommandHandler<CreatePerson, Guid>>()
            .Count()
            .ShouldBe(1);
}