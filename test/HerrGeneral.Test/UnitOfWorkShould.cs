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
            .AddHerrGeneralTestLogger(output)
            .UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping.Handler).Assembly, typeof(Ping.Handler).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                );

        var serviceProvider = services.BuildServiceProvider();
        
        await new Ping { Message = "Ping" }
            .SendFromMediator(serviceProvider.GetRequiredService<Mediator>())
            .ShouldSuccess();
    }
    
    private ServiceProvider Register(IUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IUnitOfWork>(_ => unitOfWork)
            .AddSingleton(new Dependency())
            .AddSingleton(new ReadModel())
            .UseHerrGeneral(scanner =>
                scanner
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!)
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapEventHandlerOnWriteSide<EventBase, HerrGeneral.Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
                );
                
        return services.BuildServiceProvider();
    }
    
    [Fact]
    public async Task Commit()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.Commit(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.Dispose(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
       
    [Fact]
    public async Task RollBack_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var serviceProvider = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.SendFromMediator(serviceProvider.GetRequiredService<Mediator>());
        
        A.CallTo(() => unitOfWork.RollBack(A<UnitOfWorkId>._)).MustHaveHappened();
    }
}