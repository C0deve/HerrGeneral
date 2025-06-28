# Herr General v0.1.14-rc


Herr General is a Cqrs implementation with built in debug log for simple modular monolith.
(strongly inspired from MediatR)

Send a command :
1. Handle the command
2. Dispatch events to write side
3. Dispatch events to read side
4. Return a Result


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

// Dynamic handlers registration
services.RegisterDynamicHandlers(typeof(CreatePerson).Assembly); 
```


## Sample code

```csharp
// Write Side
// Command + handler
public record record SetFriend(Guid AggregateId, string Friend) : Change<Person>(AggregateId)
{  
    public class Handler : ChangeAggregateHandler<Person,SetFriend>
    {
        public Handler(CtorParams ctorParams) : base(ctorParams) { }

        protected override Person Handle(Person aggregate, SetFriend command) => 
            aggregate.SetFriend(command._friend, command.Id);
    }
}

// or

//Command only (use dynamic command handler behind the scene)
public record record SetFriend(Guid AggregateId, string Friend) : Change<Person>(AggregateId);


// Read side
public record PersonFriendRM(Guid PersonId, string Person, string Friend)
{
    public class PersonFriendRMRepository : HerrGeneral.ReadSide.IEventHandler<FriendChanged>
    {
        ...
        
        public Task Handle(FriendChanged notification, CancellationToken cancellationToken)
        {
            ...           
        }
        
        ...
    }    
}
```


## Dynamic command handler

With dynamic command handlers you don't need to write handler.

### Nugget

[HerrGeneral.Core.DDD](https://www.nuget.org/packages/HerrGeneral.Core.DDD/)

### Registration

```csharp
// Register a dynamic handler for all commands without handler
services.RegisterDynamicHandlers(typeof(CreatePerson).Assembly);

// Register the default IAggregateFactory for aggregate creation
// DefaultAggregateFactory will call the aggregate constructor new(TCreateCommand command, Guid aggregateId)
services.AddTransient<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>();
```

### Conventions

#### Create (with DefaultAggregateFactory)

The aggregate must have a constructor new(TCreate command, Guid aggregateId).

#### Change

The aggregate must have a method Execute(TChange command).

### Behind the scene

Dynamic handler for create:
1. call IAggregateFactory.Create(Create<TAggregate> command, Guid aggregateId)
2. save the aggregate
3. dispatch events on write side
4. dispatch events on read side

Dynamic handler for change:
1. get the aggregate
2. call the execute method with command as argument
3. save the aggregate
4. dispatch events on write side
5. dispatch events on read side


## Debug logger output sample

<------------------- SetFriend <46a0deab-0485-403e-821a-834a96517a7c> thread<1> ------------------->
|| Publish Write Side on thread<1>
&emsp;HerrGeneral.SampleApplication.WriteSide.FriendChanged

|| Publish Read Side (1 event) on thread<1>
&emsp;HerrGeneral.SampleApplication.WriteSide.FriendChanged
&emsp;-> Handle by HerrGeneral.SampleApplication.ReadSide.PersonFriendRM+PersonFriendRMRepository
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

### CreateResult

Herr general return CreationResult for CreationCommand
3 states : Success<Guid>, DomainError, PanicException

```csharp
createResult.Match(id =>
    {
        ...
    },
    error => ...,
    exception => ...);
```