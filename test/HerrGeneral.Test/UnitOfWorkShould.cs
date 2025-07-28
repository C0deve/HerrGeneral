using FakeItEasy;
using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
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
            .AddSingleton<Dependency2>()
            .AddHerrGeneralTestLogger(output)
            .UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping.Handler).Assembly, typeof(Ping.Handler).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
            );

        var mediator = services
            .BuildServiceProvider()
            .GetRequiredService<Mediator>();

        await mediator
            .Send(new Ping { Message = "Ping" })
            .ShouldSuccess();
    }

    private Mediator BuildMediator(IUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IUnitOfWork>(_ => unitOfWork)
            .AddSingleton<Dependency>()
            .AddSingleton<Dependency2>()
            .AddSingleton<ReadModel>()
            .AddSingleton<ReadModelWithMultipleHandlers>()
            .AddSingleton<ReadModelWithMultipleHandlersAndInheritingIEventHandler>()
            .UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapEventHandlerOnWriteSide<EventBase, HerrGeneral.Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
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
        var ping = new Ping { Message = "Ping" };

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
        var ping = new Ping { Message = "Ping" };

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