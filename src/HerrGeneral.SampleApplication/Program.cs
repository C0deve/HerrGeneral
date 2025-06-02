using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.SampleApplication.ReadSide;
using HerrGeneral.SampleApplication.WriteSide;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var container = new Container(cfg =>
{
    cfg.AddLogging(builder => builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole());
    cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
    cfg.AddTransient<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>();
    cfg.UseHerrGeneral(configuration =>
            configuration
                .UseWriteSideAssembly(typeof(Person).Assembly, typeof(Person).Namespace!)
                .UseReadSideAssembly(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!)
        )
        .RegisterDynamicHandlers(typeof(CreatePerson).Assembly);
});
Console.WriteLine("Initialization Ok");

var mediator = container.GetInstance<Mediator>();

var smithId = Guid.Empty;

Console.WriteLine("Creating Smith");

await mediator
    .Send<Guid>(new CreatePerson("Smith", "Adams"))
    .Then(personId =>
    {
        smithId = personId;
        DisplayFriend(container, smithId);
        Console.WriteLine("Set Theo as a friend");
        return mediator.Send(new SetFriend(personId, "Theo"));
    })
    .Then(() =>
    {
        DisplayFriend(container, smithId);
        Console.WriteLine("Set Alfred as a friend");
        return mediator.Send(new SetFriend(smithId, "Alfred"));
    })
    .Match(() => DisplayFriend(container, smithId),
        _ => Console.WriteLine("Fail"),
        _ => Console.WriteLine("Fail"));


Console.ReadKey();
return;

void DisplayFriend(IServiceContext container1, Guid guid)
{
    var rm = container1.GetInstance<PersonFriendRM.PersonFriendRMRepository>().Get(guid);
    Console.WriteLine($"{rm.Person} friend is {rm.Friend}.");
}