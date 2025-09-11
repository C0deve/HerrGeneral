using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
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
            .AddSingleton<IAggregateRepository<TheThing>, Repository<TheThing>>()
            .AddSingleton<IAggregateFactory<TheThing>, DefaultAggregateFactory<TheThing>>()
            .AddHerrGeneral(configuration => 
                configuration
                    .ScanWriteSideOn(
                        typeof(TheThing).Assembly,
                        "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide"))
            .BuildServiceProvider();

    [Fact]
    public void RegisterDynamicHandlersForChangeAggregateCommandWithoutHandler() =>
        _container
            .GetServices<ICommandHandler<AChangeCommandWithoutHandler, Unit>>()
            .Count()
            .ShouldBe(1);

    [Fact]
    public void RegisterDynamicHandlersForCreateAggregateCommandWithoutHandler() =>
        _container
            .GetServices<ICommandHandler<CreateTheThingNoHandler, Guid>>()
            .Count()
            .ShouldBe(1);
}