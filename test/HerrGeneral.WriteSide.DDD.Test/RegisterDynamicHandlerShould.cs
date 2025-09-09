using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
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
            .AddSingleton<IAggregateRepository<TheAggregate>, Repository<TheAggregate>>()
            .AddSingleton<IAggregateFactory<TheAggregate>, DefaultAggregateFactory<TheAggregate>>()
            .AddHerrGeneral(scanner => scanner)
            .RegisterDDDHandlers(typeof(TheAggregate).Assembly)
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
            .GetServices<ICommandHandler<ChangeTheAggregate, Unit>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void RegisterDynamicHandlersForCreateAggregateCommandWithoutHandler() =>
        _container
            .GetServices<ICommandHandler<CreateTheAggregateNoHandler, Guid>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void NotRegisterDynamicHandlersForCreateAggregateCommandWithHandler() =>
        _container
            .GetServices<ICommandHandler<CreateTheAggregate, Guid>>()
            .Count()
            .ShouldBe(1);
}