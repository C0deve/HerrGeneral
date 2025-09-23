# HerrGeneral.WriteSide.DDD

## Overview

HerrGeneral.WriteSide.DDD provide interfaces and implementations for writing domain logic in a DDD-style.

Key elements you'll find:
- Base classes:
  - Aggregates with integrated domain events
  - Create<TAggregate>
  - Change<TAggregate>

- Interfaces:
  - **Command handlers** for processing commands
    - ICreateHandler<out TAggregate, in TCommand>
    - IChangeHandler<TAggregate, in TCommand>
    - IChangeMultiHandler<out TAggregate, in TCommand>
  - **Domain event handlers** for processing domain events
    - IDomainEventHandler<in TEvent> for processing events and returning changed aggregates.
    - IVoidDomainEventHandler<in TEvent> for processing events without returning anything.
    - ICrossAggregateChangeHandler<in TEvent, TAggregate> for processing events and returning the changes to make.
  No need to inject repository
  - **Command with automatic handler** 
  for processing commands with no explicit handler (see Convention-Based Dynamic Command Handling)
    - INoHandlerCreate<TAggregate>
    - INoHandlerChange<TAggregate>

## NuGet Package

- **[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)** (in the domain layer)

```
dotnet add package HerrGeneral.WriteSide.DDD
```

## Code Examples

HerrGeneral.WriteSide.DDD offers two approaches to handling commands. Choose the style that best fits your project needs.

### Commands Handlers

#### Creating an Aggregate

```csharp
// Define a command to create a person with all required properties
public record CreatePerson(string Name) : Create<Person>
{
    // Nested handler class provides implementation for this specific command
    public class Handler : ICreateHandler<Person, CreatePerson>
    {
        // Handle method creates and returns the new aggregate instance
        public Person Handle(CreatePerson command, Guid aggregateId) => 
            new(aggregateId, command.Name, command.Id);
    }
}
```
or without explicit handler class:
```csharp
// Just define the command inheriting from INoHandlerCreate<>
public record CreatePerson(string Name) : Create<Person>, 
    INoHandlerCreate<Person>;

// Your aggregate should provide a constructor that accepts (Command, AggregateId) as parameters
public class Person
{
    // This constructor will be automatically discovered and called by the dynamic handler system
    public Person(CreatePerson command, Guid aggregateId)
    {
        // ...
    }
}
```

#### Changing Aggregate State

```csharp
// Define a command to create a Person aggregate
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId);
{
    public class Handler : IChangeHandler<Person, SetFriend>
    {
        public Person Handle(Person aggregate, SetFriend command)
        { 
            // Your domain logic goes here
            aggregate.ChangeName(command.Friend, command.Id);
            
            // The framework handles persistence and event dispatching
            return aggregate;
        }
    }
}
```
or without explicit handler class:
```csharp
// Just define the command inheriting from INoHandlerChange<>
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId)
    , INoHandlerChange<Person>;

// Your aggregate implements the matching Execute method
public class Person
{
    // This method will be automatically discovered and called by the dynamic handler system
    public Person Execute(ChangeName command)
    {
        // ..
        
        // The framework handles persistence and event dispatching
        return this;
    }
}
  
```
### Event Handlers

See [sample application](https://github.com/C0deve/HerrGeneral/tree/main/src/HerrGeneral.SampleApplication/Bank) for more details.


### Required Package

- **[HerrGeneral.Core.DDD](https://www.nuget.org/packages/HerrGeneral.Core.DDD/)** (in the infrastructure layer):  
  The engine of HerrGeneral framework. Needed for adding HerrGeneral to the service container and send commands through the mediator.

```bash
dotnet add package HerrGeneral.Core.DDD
```