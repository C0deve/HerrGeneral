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
            cfg.UseHerrGeneral(scanner =>
                    scanner.OnWriteSide(typeof(Person).Assembly, typeof(Person).Namespace!))
                .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly);
        });
    }

    [Fact]
    public async Task HandleAChangeCommandWithoutHandler()
    {
        var personId = await new CreatePerson("John").Send(_container);
        await new AChangeCommandWithoutHandler("Remy", personId).Send(_container);
    }
    
    [Fact]
    public async Task HandleACreateCommandWithoutHandler() => 
        await new ACreateCommandWithoutHandler("John").Send(_container);
}