using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using Lamar;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class DynamicHandlerHandlerShould
{
    private readonly Container _container;

    public DynamicHandlerHandlerShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.For<IAggregateFactory<Person>>().Use<DefaultAggregateFactory<Person>>();
            cfg.UseHerrGeneral(scanner =>
                    scanner.AddWriteSideAssembly(typeof(Person).Assembly, typeof(Person).Namespace!))
                .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly);
        });
    }

    [Fact]
    public async Task HandleAChangeCommandWithoutHandler()
    {
        var personId = await new CreatePerson("John").Send<Guid>(_container);
        await new AChangeCommandWithoutHandler("Remy", personId).Send(_container);
    }
    
    [Fact]
    public async Task HandleASecondChangeCommandWithoutHandler()
    {
        var personId = await new CreatePerson("John").Send<Guid>(_container);
        await new ASecondChangeCommandWithoutHandler("Remy", personId).Send(_container);
    }
    
    [Fact]
    public async Task HandleACreateCommandWithoutHandler() => 
        await new ACreateCommandWithoutHandler("John").Send<Guid>(_container);
    
    [Fact]
    public async Task ThrowIfExecuteMethodNotFound()
    {
        var personId = await new CreatePerson("John").Send<Guid>(_container);
        await new AThirdChangeCommandWithoutHandler("Remy", personId).Send(_container, false)
            .ShouldHavePanicExceptionOfType<MissingMethodException>();
    }
    
    [Fact]
    public async Task ThrowIfConstructorNotFound() =>
        await new ASecondCreateCommandWithoutHandler("John").Send<Guid>(_container, false)
            .ShouldHavePanicExceptionOfType<MissingMethodException, Guid>();
}