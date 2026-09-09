# HerrGeneral.WriteSide.DDD

## Overview

HerrGeneral.WriteSide.DDD provides interfaces, base classes, and attributes for writing domain logic in a DDD style.

Key elements:
- **Base classes**:
  - Aggregates with integrated domain events (`IAggregate`)
  - `Create<TAggregate>`
  - `Change<TAggregate>` (automatically keyed with `(typeof(TAggregate), AggregateId)`)

- **Concurrency & Locking**:
  - `[AggregateLockKey<TAggregate>]` attribute: Strongly-typed lock key constrained to `IAggregate`.

- **Interfaces**:
  - **Command handlers** for processing commands:
    - `ICreateHandler<out TAggregate, in TCommand>`
    - `IChangeHandler<TAggregate, in TCommand>`
    - `IChangeMultiHandler<out TAggregate, in TCommand>`
  - **Domain event handlers** for processing domain events:
    - `IDomainEventHandler<in TEvent>` for processing events and returning changed aggregates.
    - `IVoidDomainEventHandler<in TEvent>` for processing events without returning anything.
    - `ICrossAggregateChangeHandler<in TEvent, TAggregate>` for processing events and returning the changes to make.
  - **Command with automatic handler** (Convention-Based Dynamic Command Handling):
    - `INoHandlerCreate<TAggregate>`
    - `INoHandlerChange<TAggregate>`

## NuGet Package

- **[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)** (in the domain layer)

```bash
dotnet add package HerrGeneral.WriteSide.DDD
```

## Command Concurrency & Locking in DDD

Commands targeting the same aggregate instance are automatically serialized to prevent race conditions while allowing concurrent processing across different aggregates.

### 1. Built-in with `Change<TAggregate>`

When using the `Change<TAggregate>` base class, partitioning is handled automatically:

```csharp
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId);
```

### 2. Explicit with `[AggregateLockKey<TAggregate>]`

For custom commands that do not inherit from `Change<TAggregate>`, use `[AggregateLockKey<TAggregate>]` on the aggregate identifier:

```csharp
using HerrGeneral.DDD;

public record CancelOrder(
    [property: AggregateLockKey<Order>] Guid OrderId, 
    string Reason
);
```

This enforces at compile-time that `Order` implements `IAggregate` and scopes the lock to `(typeof(Order), OrderId)`.

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
// Define a command to change a Person aggregate
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId)
{
    public class Handler : IChangeHandler<Person, ChangeName>
    {
        public Person Handle(Person aggregate, ChangeName command)
        { 
            // Your domain logic goes here
            aggregate.ChangeName(command.NewName);
            
            // The framework handles persistence and event dispatching
            return aggregate;
        }
    }
}
```
or without explicit handler class:
```csharp
// Just define the command inheriting from INoHandlerChange<>
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId), 
    INoHandlerChange<Person>;

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
