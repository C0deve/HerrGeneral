# Herr General

Herr General is a Cqrs implementation with built in debug log for simple modular monolith.
(strongly inspired from MediatR)

## Implementation choice

One storage for all
pro : no eventual consistency
cons : no eventual consistency => doesn't scale

One transaction by command.

ReadModel as singleton.

All ids are System.Guid.

Result pattern but CreationResult return the id of the created object.

## Installing Herr General with NuGet

Write side
[HerrGeneral.WriteSide](https://www.nuget.org/packages/HerrGeneral.WriteSide/)
or
[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)

Read side
[HerrGeneral.ReadSide](https://www.nuget.org/packages/HerrGeneral.ReadSide/)

Application or infrastructure layer
[HerrGeneral.Core](https://www.nuget.org/packages/HerrGeneral.Core/)

### Registering with `IServiceCollection`

Herr General supports `Microsoft.Extensions.DependencyInjection.Abstractions` directly.
To register write side commands and eventHandlers and read side eventHandlers:

```csharp
services.UseHerrGeneral(scanner =>
        scanner
            .OnWriteSide(typeof(Person).Assembly, typeof(Person).Namespace!)
            .OnReadSide(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!));
```

### Sample code

```csharp
// Write Side
public record SetFriend : ChangeAggregate<Person>
{
    private readonly string _friend;

    public SetFriend(Guid aggregateId, string friend) : base(aggregateId) => _friend = friend;

    public class Handler : ChangeAggregateHandler<Person,SetFriend>
    {
        public Handler(CtorParams ctorParams) : base(ctorParams) { }

        protected override Person Handle(Person aggregate, SetFriend command) => 
            aggregate.SetFriend(command._friend, command.Id);
    }
}

// Read side
public record PersonFriendRM(Guid PersonId, string Person, string Friend)
{
    public class PersonFriendRMRepository : IEventHandler<FriendChanged>
    {
        private readonly Dictionary<Guid, PersonFriendRM> _personFriends = new();
        public Task Handle(FriendChanged notification, CancellationToken cancellationToken)
        {
            _personFriends[notification.AggregateId] = new PersonFriendRM(notification.AggregateId, notification.Person, notification.FriendName);
            return Task.CompletedTask;
        }

        public PersonFriendRM Get(Guid personId) => _personFriends[personId];
    }    
}
```

## Debug logger output sample

<------------------- SetFriend <46a0deab-0485-403e-821a-834a96517a7c> thread<1> ------------------->
|| Publish Write Side on thread<1>
HerrGeneral.SampleApplication.WriteSide.FriendChanged

|| Publish Read Side (1 event) on thread<1>
HerrGeneral.SampleApplication.WriteSide.FriendChanged
-> Handle by HerrGeneral.SampleApplication.ReadSide.PersonFriendRM+PersonFriendRMRepository
<------------------- SetFriend Finished 00:00:00.0021475 -------------------/>

## How it works

A user: Hey, I want to change my friend name.
Application: Ok, give me the command and I will take care of the rest. I'll send you back a commandResult when I'm done.

In the application black box :
Mediator: Anybody to handle this command ?
CommandHandler: I"m here.
CommandHandler: I'm done and I have some events to publish.
WriteSideEventPublisher: That's my job.
WriteSideEventPublisher: Anybody to handle this event on the write side ? (for each event).
WriteSideEventHandler: Me (and I may have other events to publish).
WriteSideEventPublisher: I'm done.
CommandHandler: Thank you, now I can transmit all those events to the read side.
ReadSideEventPublisher: Anybody to handle this event on the read side ? (for each event).
ReadModel: Yes me.
ReadSideEventPublisher: I'm done.
CommandHandler: I'm done.

Application: Here is your command result.

![HowItWorks.png](..\assets\HowItWorks.png)

## Result pattern

### ChangeResult

Herr general return ChangeResult for ChangeCommand
3 states : Success, DomainError, PanicException

```csharp
updateResult.Match(() =>
    {
        ...
    },
    error => ...,
    exception => ...);
```

### CreationResult

Herr general return CreationResult for CreationCommand
3 states : Success<Guid>, DomainError, PanicException

```csharp
creationResult.Match(id =>
    {
        ...
    },
    error => ...,
    exception => ...);
```







Work in progress
