
using FakeItEasy;
using HerrGeneral.Registration;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using HerrGeneral.Test.Extension.Internal;
using HerrGeneral.WriteSide;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.UnitOfWork.Test;

public class UnitOfWorkShould
{
    private readonly ITestOutputHelper _output;

    public UnitOfWorkShould(ITestOutputHelper output) => _output = output;
    
    [Fact]
    public async Task Not_be_mandatory()
    {
        var container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(_output);

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .OnWriteSide(typeof(Ping.Handler).Assembly, typeof(Ping.Handler).Namespace!));
        });

        
        var result = await new Ping { Message = "Ping" }.Send(container, false);
        result.ShouldBe(CommandResultV2.Success);
    }
    
    private Container Register(IUnitOfWork unitOfWork) =>
        new(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(_output);
            
            cfg.ForSingletonOf<IUnitOfWork>().Use(_ => unitOfWork);
            cfg.ForSingletonOf<Dependency>().Use(new Dependency());
            cfg.ForSingletonOf<ReadModel>().Use(new ReadModel());

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .OnWriteSide(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .OnReadSide(typeof(Ping).Assembly, typeof(ReadModel).Namespace!));
        });
    
    [Fact]
    public async Task Commit()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.Send(container);
        
        A.CallTo(() => unitOfWork.Commit(ping.Id)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new Ping { Message = "Ping" };
        
        await ping.Send(container);
        
        A.CallTo(() => unitOfWork.Dispose(ping.Id)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.Dispose(ping.Id)).MustHaveHappened();
    }

    [Fact]
    public async Task Dispose_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.Dispose(ping.Id)).MustHaveHappened();
    }
    
    [Fact]
    public async Task Dispose_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.Dispose(ping.Id)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_domain_error_thrown_from_command_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInCommandHandler();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.RollBack(ping.Id)).MustHaveHappened();
    }
    
    [Fact]
    public async Task RollBack_on_on_domain_error_thrown_from_event_handler()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithFailureInEventHandler();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.RollBack(ping.Id)).MustHaveHappened();
    }
       
    [Fact]
    public async Task RollBack_on_on_panic_exception() 
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var container = Register(unitOfWork);
        var ping = new PingWithPanicException();
        
        await ping.Send(container, false);
        
        A.CallTo(() => unitOfWork.RollBack(ping.Id)).MustHaveHappened();
    }
}