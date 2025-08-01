using FakeItEasy;
using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.WithHerrGeneralDependency.ReadSide;
using HerrGeneral.Test.Data.WithMapping.ReadSide;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.UnitOfWork.Test;

public class UnitOfWorkShould(ITestOutputHelper output)
{
    [Fact]
    public async Task Not_be_mandatory()
    {
        var services = new ServiceCollection()
            .AddSingleton<CommandTracker1>()
            .AddSingleton<CommandTracker2>()
            .AddSingleton<CommandTracker3>()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanWriteSideOn(typeof(Ping.Handler).Assembly, typeof(Ping.Handler).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
            );

        var mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();

        await mediator
            .Send(new Ping())
            .ShouldSuccess();
    }

    private Mediator BuildMediator(IUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IUnitOfWork>(_ => unitOfWork)
            .AddSingleton<CommandTracker1>()
            .AddSingleton<CommandTracker2>()
            .AddSingleton<CommandTracker3>()
            .AddSingleton<AReadModelWithMultipleHandlers>()
            .AddSingleton<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanWriteSideOn(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .ScanReadSideOn(typeof(Ping).Assembly, typeof(AReadModel).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapWriteSideEventHandler<EventBase, HerrGeneral.Test.Data.WithMapping.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapReadSideEventHandler<EventBase, HerrGeneral.Test.Data.WithMapping.ReadSide.ILocalEventHandler<EventBase>>()
            );

        return services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task Commit()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new Ping();

        await mediator
            .Send(ping)
            .ShouldSuccess();

        A.CallTo(() => unitOfWork.Commit(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new Ping();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithFailureInEventHandler();

        await mediator.Send(ping);


        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_on_panic_exception()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithPanicException();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task RollBack_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task RollBack_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithFailureInEventHandler();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task RollBack_on_on_panic_exception()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var mediator = BuildMediator(unitOfWork);
        var ping = new PingWithPanicException();

        await mediator.Send(ping);

        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
}