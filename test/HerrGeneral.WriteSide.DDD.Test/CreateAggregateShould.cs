using HerrGeneral.Core.DDD;
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
            cfg.UseHerrGeneral(configuration =>
                configuration
                    .UseWriteSideAssembly(typeof(Person).Assembly, typeof(Person).Namespace!)
                    .MapAllDDDHandlers<Person>());
        });
    }

    [Fact]
    public async Task Create() =>
        await new CreatePerson("John").Send<Guid>(_container);
}