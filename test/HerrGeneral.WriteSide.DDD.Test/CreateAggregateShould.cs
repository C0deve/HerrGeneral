using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using Xunit.Abstractions;
using Lamar;

namespace HerrGeneral.WriteSide.DDD.Test;

public class CreateAggregateShould
{
    private readonly Container _container;

    public CreateAggregateShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.UseHerrGeneral(scanner =>
                scanner.AddWriteSideAssembly(typeof(Person).Assembly, typeof(Person).Namespace!));
        });
    }
    
    [Fact]
    public async Task Create() => 
        await new CreatePerson("John").Send<Guid>(_container);
}