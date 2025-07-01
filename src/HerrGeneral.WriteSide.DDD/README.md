# HerrGeneral.WriteSide.DDD

## Overview

HerrGeneral WriteSide DDD is a streamlined implementation of Domain Driven Design and CQRS write-side concepts. 
This library aims to reduce boilerplate code while maintaining clean domain modeling practices.

Key components you'll find:
- **Rich aggregates** with integrated domain events
- **Creation commands** with dedicated handlers for aggregate instantiation
- **State change commands** with handlers for modifying aggregate state

## NuGet Packages

- **[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)**: Extended support for Domain-Driven Design patterns on the write side

## Registration

Integrating HerrGeneral.WriteSide.DDD into your application is straightforward with these registration steps:

```csharp
// 1. Register command and domain event handlers from your domain assembly
services.RegisterDDDHandlers(typeof(CreatePerson).Assembly);

// 2. (Optional) Enable convention-based dynamic handlers to further reduce boilerplate
// This registers handlers automatically for commands that follow conventions
services.RegisterDynamicHandlers(typeof(CreatePerson).Assembly);

// 3. Register the default aggregate factory for creating new aggregates
// This factory automatically calls the appropriate constructor on your aggregate
services.AddTransient<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>();
```

This registration process intelligently scans your specified assemblies for command handlers and event handlers, registering them with the appropriate lifetime scopes in the dependency injection container.

## Code Examples

HerrGeneral.WriteSide.DDD offers two approaches to handling commands. Choose the style that best fits your project needs.

### Option 1: Commands with Explicit Handlers

This traditional approach gives you full control over command handling logic with explicit handler classes.

#### Creating an Aggregate

```csharp
// Define a command to create a person with all required properties
public record CreatePerson(string Name, string Friend) : Create<Person>
{
    // Nested handler class provides implementation for this specific command
    public class Handler : ICreateHandler<Person, CreatePerson>
    {
        // Handle method creates and returns the new aggregate instance
        public Person Handle(CreatePerson command, Guid aggregateId) => 
            new(aggregateId, command.Name, command.Friend, command.Id);
    }
}
```

#### Changing Aggregate State

```csharp
// Define a command to modify an existing Person aggregate
public record SetFriend(Guid AggregateId, string Friend) : Change<Person>(AggregateId)
{  
    // Explicit handler implementation for modifying the aggregate
    public class Handler : IChangeHandler<Person, SetFriend>
    {
        // Handle method applies changes to the aggregate and returns the updated version
        public Person Handle(Person aggregate, SetFriend command)
        { 
            // Call domain method that encapsulates the business logic for this change
            aggregate.SetFriend(command.Friend, command.Id);
            return aggregate;
        }
    }
}
```

### Option 2: Commands with Dynamic Handlers (Convention-Based)

## Convention-Based Dynamic Command Handling

HerrGeneral offers a powerful convention-based approach that automatically generates command handlers at runtime. This eliminates handler boilerplate entirely when you follow simple naming conventions.

```csharp
// Just define the command - no handler class needed!
public record SetFriend(Guid AggregateId, string Friend) : Change<Person>(AggregateId);

// Your aggregate implements the matching Execute method
public class Person
{
    // This method will be automatically discovered and called by the dynamic handler system
    public Person Execute(SetFriend command)
    {
        // Your domain logic goes here
        this.Friend = command.Friend;
        // The framework handles persistence and event dispatching
        return this;
    }
}
```

### Convention Rules Explained

Dynamic handlers rely on these straightforward naming and signature conventions in your domain model:

#### For Create Commands

When using the `DefaultAggregateFactory`, your aggregate must provide a constructor matching this pattern:

```csharp
// Constructor signature pattern for any command that inherits from Create<Person>
public Person(CreatePerson command, Guid aggregateId)
{
    // Constructor should initialize all required state from the command
    this.Id = aggregateId;
    this.Name = command.Name;
    this.Friend = command.Friend;

    // Optionally raise domain events
    AddDomainEvent(new PersonCreated(this.Id, this.Name));
}
```

#### For Change Commands

Your aggregate must implement an `Execute` method that accepts the specific command type:

```csharp
// Execute method pattern for any command that inherits from Change<Person>
public Person Execute(SetFriend command)
{
    // Validate business rules
    if (string.IsNullOrEmpty(command.Friend))
        throw new InvalidFriendNameException("Friend name cannot be empty");

    // Apply the change
    string oldFriend = this.Friend;
    this.Friend = command.Friend;

    // Optionally raise domain events
    AddDomainEvent(new FriendChanged(this.Id, oldFriend, this.Friend));

    // Return the updated aggregate (this is required)
    return this;
}
```

### Behind the Scenes: How Dynamic Handlers Work

#### For Create Commands

When a creation command is processed, the dynamic handler system:

1. **Factory Invocation**: Calls `IAggregateFactory.Create(Create<TAggregate> command, Guid aggregateId)` which invokes your aggregate's matching constructor
2. **Persistence**: Automatically saves the newly created aggregate to the configured repository
3. **Write-Side Event Dispatch**: Routes any domain events to write-side event handlers for additional domain logic
4. **Read-Side Event Dispatch**: Forwards events to read-side handlers for projection updates

#### For Change Commands

When a state change command is processed, the dynamic handler system:

1. **Retrieval**: Automatically loads the target aggregate from the repository using the AggregateId
2. **Method Invocation**: Calls the matching `Execute(TCommand command)` method on your aggregate
3. **Persistence**: Saves the updated aggregate back to the repository
4. **Write-Side Event Dispatch**: Routes any new domain events to write-side event handlers
5. **Read-Side Event Dispatch**: Forwards events to read-side handlers for projection updates


### The Command Journey: A Conversational Walkthrough

Let's follow a command through the entire processing pipeline to better understand how everything connects:

**User Interface**: "User wants to change their friend's name to 'Alice'."

**API Controller**: "I'll create a `SetFriend` command and send it through the mediator."

**Behind the scenes**:

1. **Mediator**: "I've received a `SetFriend` command. Let me find the appropriate handler."
2. **Dynamic Command Handler**: "I'll handle this command for the Person aggregate."
3. **Repository**: "Let me load the Person aggregate with ID 'abc-123'."
4. **Dynamic Handler**: "Now I'll call the `Execute(SetFriend)` method on the Person aggregate."
5. **Person Aggregate**: "I'm changing the friend name and raising a `FriendChanged` domain event."
6. **Repository**: "I'll save the updated Person aggregate."
7. **WriteSideEventDispatcher**: "I'm sending the `FriendChanged` event to all interested domain services."
8. **Domain Service**: "I need to validate this friendship in my write-side event handler."
9. **ReadSideEventDispatcher**: "Now I'll distribute the event to read model handlers."
10. **FriendListProjection**: "I'll update the friend list view to show the new name."
11. **NotificationHandler**: "I'll prepare a notification about this friend change."
12. **Handler Completion**: "All processing is complete, returning a Success result."

**API Controller**: "I've received a successful result and will return HTTP 200 with confirmation details."

**User Interface**: "Friend name successfully updated to 'Alice'!"



### Required Package

To use dynamic command handlers, install the dedicated package:

```
dotnet add package HerrGeneral.Core.DDD
```

## Benefits of Using HerrGeneral.WriteSide.DDD

- **Reduced Boilerplate**: Write significantly less infrastructure code with convention-based handlers
- **Domain-Focused**: Keep your domain model clean and focused on business logic
- **Flexible Implementation**: Choose between explicit handlers for complex scenarios or dynamic handlers for simpler cases
- **Consistent Event Handling**: Automatic event dispatching ensures all domain events are properly processed
- **Transaction Management**: All operations within a command are executed in a single transaction
- **Clean Architecture Support**: Promotes separation of concerns between domain and infrastructure
- **Progressive Adoption**: Can be introduced gradually into existing codebases

## Common Usage Patterns

- **Aggregate Root Management**: Perfect for creating and modifying aggregate roots following DDD principles
- **CQRS Write Side**: Pairs well with separate read models for a full CQRS implementation
- **Domain Event Sourcing**: Built-in domain event support complements event sourcing patterns
- **Bounded Context Integration**: Helps maintain clean boundaries between different domain areas