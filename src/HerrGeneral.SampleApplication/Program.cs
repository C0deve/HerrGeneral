// See https://aka.ms/new-console-template for more information

using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.SampleApplication.ReadSide;
using HerrGeneral.SampleApplication.WriteSide;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("Hello, World!");
var container = new Container(cfg =>
{
    cfg.AddLogging(builder => builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole());
    cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
    cfg.UseHerrGeneral(scanner =>
        scanner
            .OnWriteSide(typeof(Person).Assembly, typeof(Person).Namespace!)
            .OnReadSide(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!));
});
Console.WriteLine("Initialization Ok");

var mediator = container.GetInstance<Mediator>();

Console.WriteLine("Creating Smith");

var result = await mediator.Send(new CreatePerson("Smith", "Adams"));

var personId = Guid.Empty;

var setFriendResult = await result.Match(guid =>
    {
        personId = guid;
        DisplayFriend(container, personId);
        Console.WriteLine("Set theo as a friend");
        return mediator.Send(new SetFriend(guid, "Theo"));
    },
    error => Task.FromResult(ChangeResult.DomainFail(error)),
    exception => Task.FromResult(ChangeResult.PanicFail(exception)));

setFriendResult.Match(() =>
    {
        Console.WriteLine("Success");
        DisplayFriend(container, personId);
    },
    _ => Console.WriteLine("Fail"),
    _ => Console.WriteLine("Fail"));

Console.ReadKey();

void DisplayFriend(IServiceContext container1, Guid guid)
{
    var rm = container1.GetInstance<PersonFriendRM.PersonFriendRMRepository>().Get(guid);
    Console.WriteLine($"{rm.Person} friend is {rm.Friend}.");
}