using FakeItEasy;
using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using Lamar;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.UnitOfWork.Test;

public class UnitOfWorkShould(ITestOutputHelper output)
{
    [Fact]
    public async Task Not_be_mandatory()
    {
        var container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping.Handler).Assembly, typeof(Ping.Handler).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                );
        });

        
        await new Ping { Message = "Ping" }
            .SendFromMediator(container.GetInstance<Mediator>())
            .ShouldSuccess();
    }
    
    private Container Register(IUnitOfWork unitOfWork) =>
        new(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            
            cfg.ForSingletonOf<IUnitOfWork>().Use(_ => unitOfWork);
            cfg.ForSingletonOf<Dependency>().Use(new Dependency());
            cfg.ForSingletonOf<ReadModel>().Use(new ReadModel());

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapEventHandlerOnWriteSide<EventBase, HerrGeneral.Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
                );
        });
    
    [Fact]
    public async Task Commit()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.Commit(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
       
    [Fact]
    public async Task RollBack_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.SendFromMediator(container.GetInstance<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
}