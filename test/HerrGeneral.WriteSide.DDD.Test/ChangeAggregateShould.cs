using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using Lamar;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class ChangeAggregateShould
{
    private readonly Container _container;

    public ChangeAggregateShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.UseHerrGeneral(scanner =>
                scanner.OnWriteSide(typeof(Person).Assembly, typeof(Person).Namespace!));
        });
    }
    [Fact]
    public async Task Change()
    {
        var personId = await new CreatePerson().Send(_container);
        await new AddFriend(personId).Send(_container);
    }
}