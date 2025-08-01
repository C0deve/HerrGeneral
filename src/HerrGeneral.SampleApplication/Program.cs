using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.SampleApplication.ReadSide;
using HerrGeneral.SampleApplication.WriteSide;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection()
    .AddLogging(builder => builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole())
    .AddSingleton<IAggregateRepository<Person>, PersonRepository>()
    .AddTransient<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>()
    .AddHerrGeneral(configuration =>
            configuration
                .ScanReadSideOn(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!)
        )
    .RegisterDynamicHandlers(typeof(CreatePerson).Assembly);

var serviceProvider = services.BuildServiceProvider();
Console.WriteLine("Initialization Ok");

var mediator = serviceProvider.GetRequiredService<Mediator>();

var smithId = Guid.Empty;

Console.WriteLine("Creating Smith");

await mediator
    .Send<Guid>(new CreatePerson("Smith", "Adams"))
    .Then(personId =>
    {
        smithId = personId;
        DisplayFriend(serviceProvider, smithId);
        Console.WriteLine("Set Theo as a friend");
        return mediator.Send(new SetFriend(personId, "Theo"));
    })
    .Then(() =>
    {
        DisplayFriend(serviceProvider, smithId);
        Console.WriteLine("Set Alfred as a friend");
        return mediator.Send(new SetFriend(smithId, "Alfred"));
    })
    .Match(() => DisplayFriend(serviceProvider, smithId),
        _ => Console.WriteLine("Fail"),
        _ => Console.WriteLine("Fail"));


Console.ReadKey();
return;

void DisplayFriend(IServiceProvider serviceProvider1, Guid guid)
{
    var rm = serviceProvider1.GetRequiredService<PersonFriendRM.PersonFriendRMRepository>().Get(guid);
    Console.WriteLine($"{rm.Person} friend is {rm.Friend}.");
}