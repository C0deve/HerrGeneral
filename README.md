# Herr General v0.1.14-rc

## Overview

Herr General is a lightweight CQRS (Command Query Responsibility Segregation) implementation designed for building modular monolithic applications in .NET. It provides a structured approach to handling commands and events with built-in debug logging and unit of work.

> Inspired by MediatR but optimized for CQRS workflows with explicit write and read side segregation.

## Key Features

- **Clean CQRS Implementation**: Separates command and query responsibilities for better maintainability
- **Built-in Diagnostics**: Comprehensive debug logging for easy troubleshooting
- **Simple Integration**: Easy to integrate with Microsoft Dependency Injection
- **No dependency on HerrGeneral in your code**: HerrGeneral can map your handlers, no need to inherit from a ICommandHandler or IEventHandler
- **Result Pattern**: Uses a functional-style result pattern

## Command Processing Flow

```
┌─────────────┐     ┌─────────────┐     ┌───────────────────┐     ┌────────────────────────────┐
│   Client    │────▶│   Mediator  │────▶│  Command Handler  │────▶│ Write Side Events Handlers │
└─────────────┘     └─────────────┘     └───────────────────┘     └─────────────────┬──────────┘
                                                                                    │
┌─────────────┐     ┌─────────────────────┐     ┌───────────────┐                   │
│   Result    │◀────│  Command Completed  │◀────│ Read Side     │◀──────────────────┘
└─────────────┘     └─────────────────────┘     │ Event Handlers│
                                                └───────────────┘
```

When you send a command through Herr General, it follows this sequential flow:

1. The command is received by the mediator and routed to its appropriate handler
2. The command handler processes the command and generates events
3. Events are dispatched to write side handlers (domain logic) which may generate additional events
4. Events are dispatched to read side handlers (projections/views)
5. A strongly-typed `Result` or `Result<T>` is returned to the caller with the outcome

## Debug logger output sample

```
<------------------- SetFriend <46a0deab-0485-403e-821a-834a96517a7c> thread<1> ------------------->
|| Publish Write Side on thread<1>
  HerrGeneral.SampleApplication.WriteSide.FriendChanged

|| Publish Read Side (1 event) on thread<1>
  HerrGeneral.SampleApplication.WriteSide.FriendChanged
  -> Handle by HerrGeneral.SampleApplication.ReadSide.PersonFriendRM+PersonFriendRMRepository
<------------------- SetFriend Finished 00:00:00.0021475 -------------------/>
```
## Design Decisions

### Architectural Approach

- **Unified Storage**: Uses a single storage mechanism for both write and read models
  - **Advantage**: Maintains immediate consistency across all parts of the system
  - **Consideration**: This approach prioritizes consistency over extreme scalability

- **Transactional Integrity**: Each command executes within its own transaction scope
  - **Benefit**: Ensures all related changes are committed together or rolled back completely

## Installation

Herr General is distributed as a set of focused NuGet packages to allow selective adoption of its features.

### NuGet Packages

#### Core Infrastructure
- **[HerrGeneral.Core](https://www.nuget.org/packages/HerrGeneral.Core/)**: Essential components for application integration and configuration

#### Optional: Write Side Components
- **[HerrGeneral.WriteSide](https://www.nuget.org/packages/HerrGeneral.WriteSide/)**: Provides write-side interfaces for handling commands and domain events

#### Optional: Read Side Components
- **[HerrGeneral.ReadSide](https://www.nuget.org/packages/HerrGeneral.ReadSide/)**: Provides read-side `IProjectionEventHandler` for handling domain events

#### Optional: DDD Components
- **[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)**: Handlers and infrastructure for building and maintaining read models
- **[HerrGeneral.WriteSide.Core.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.Core.DDD/)**: Handlers and infrastructure for building and maintaining read models


## Getting Started

### Registration with Dependency Injection

Herr General integrates seamlessly with .NET's dependency injection system using `Microsoft.Extensions.DependencyInjection`. Configure the framework in your application startup with a fluent API:

```csharp
// Add Herr General to your service collection
services.UseHerrGeneral(configuration =>
    configuration
        // Register write side assembly and namespace for command and domain event handlers
        .UseWriteSideAssembly(typeof(Person).Assembly, typeof(Person).Namespace!)
        // Register read side assembly and namespace for read model event handlers
        .UseReadSideAssembly(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!));
```

This registration process scans the specified assemblies for command handlers and event handlers, registering them with the appropriate lifetime scopes in the dependency injection container.


## External Handler Integration

Herr General provides a comprehensive set of configuration methods to integrate existing handlers or third-party components that don't follow the standard conventions. This flexibility is essential when working with legacy systems or external libraries.

### Command Handler Integration Methods

The `Configuration` class offers several methods to adapt different types of external command handlers:

#### 1. Simple Command Handler Registration

For handlers that already return events in the expected format (`IEnumerable<object>`):

```csharp
services.UseHerrGeneral(config => 
    config.MapCommandHandler<MyCommand, ExternalCommandHandler>());
```

**How it works:**
- Registers `ExternalCommandHandler` as the handler for `MyCommand`
- The handler's return value is used directly as the event collection
- Suitable for handlers that already return a collection of events

#### 2. Command Handler with Event Mapping

For handlers that return a custom result type that needs transformation:

```csharp
services.UseHerrGeneral(config => 
    config.MapCommandHandler<MyCommand, ExternalCommandHandler, CustomResult>(
        // Transform function: converts the handler result into events
        result => result.MyEvents
    ));
```

**How it works:**
- The handler returns a custom type (`CustomResult`)
- The provided mapping function transforms this result into a collection of events
- Enables integration with handlers that don't directly produce events

#### 3. Command Handler with Value Return

For handlers that need to produce both events and a specific return value (especially useful for creation commands):

```csharp
services.UseHerrGeneral(config => 
    config.MapCommandHandler<CreateEntity, ExternalCreateHandler, CustomResult, Guid>(
        // Event mapping function
        result => result.MyEvents,
        
        // Value mapping function
        result => result.Id // Returns the ID to the caller
    ));
```

**How it works:**
- The first function maps the handler result to domain events
- The second function extracts a specific value from the result to return to the caller
- Perfect for creation commands where you need to return the new entity's ID
- The extracted value is wrapped in a `Result<TValue>` for consistent error handling

### Event Handler Integration

Event handlers can also be externally registered for both write and read sides:

#### Write Side Event Handlers

```csharp
services.UseHerrGeneral(config =>
    config.MapEventHandlerOnWriteSide<PaymentReceived, UpdateAccountBalanceHandler>());
```

**Purpose:** Register handlers that perform domain logic in response to events (may generate additional events).

#### Read Side Event Handlers

```csharp
services.UseHerrGeneral(config =>
    config.MapEventHandlerOnReadSide<OrderShipped, UpdateShippingStatisticsHandler>());
```

**Purpose:** Register handlers that update read models and projections (should not produce new events).

### Domain Exception Registration

Herr General distinguishes between technical failures and business rule violations by registering domain exceptions:

```csharp
services.UseHerrGeneral(config =>
    config.UseDomainException<InsufficientFundsException>());
```

**Benefits:**
- Registered exceptions are treated as expected business outcomes rather than system errors
- They are automatically wrapped in a `DomainError` result instead of an `Exception` result
- Provides a cleaner separation between technical failures and business rule violations

## Result Pattern

Herr General implements a functional-style result pattern to handle command outcomes in a type-safe manner. This approach eliminates exception-based control flow and provides explicit handling for success, domain errors, and system exceptions.

### Command Result Types

The framework uses two primary result types:

#### Result

Returned for commands that don't need to return a specific value (typically modification operations):

```csharp
// Result has three possible states: Success, DomainError, or Exception
public async Task ProcessUpdateCommand()
{
    // Result is returned for commands that modify existing entities
    var result = await _mediator.Send(new SetFriend(personId, "New Friend"));

    result.Match(
        onSuccess: () => {
            // Command succeeded - handle success case
            Console.WriteLine("Friend updated successfully");
        },
        onDomainError: error => {
            // Business rule validation failed
            Console.WriteLine($"Cannot update friend: {error.Message}");
        },
        onException: exception => {
            // Unexpected system error occurred
            Console.WriteLine($"System error: {exception.Message}");
            // Log the exception
        }
    );
}
```

#### Result\<T>

Returned for commands that need to return a specific value (typically creation operations):

```csharp
// Result<T> includes a value on success (typically an ID for new entities)
public async Task<Guid> ProcessCreateCommand()
{
    // Result<Guid> is returned for commands that create new entities
    var result = await _mediator.Send(new CreatePerson("John", "Doe"));

    return result.Match(
        onSuccess: id => {
            // Command succeeded with a value - we have the new entity's ID
            Console.WriteLine($"Person created with ID: {id}");
            return id; // The value can be used for further processing
        },
        onDomainError: error => {
            // Business rule validation failed
            Console.WriteLine($"Cannot create person: {error.Message}");
            return Guid.Empty;
        },
        onException: exception => {
            // Unexpected system error occurred
            Console.WriteLine($"System error: {exception.Message}");
            // Log the exception
            return Guid.Empty;
        }
    );
}
```

### Benefits of the Result Pattern

- **Explicit Error Handling**: Forces developers to consider all possible outcomes
- **Clear Intent**: Makes the code more readable by showing all possible outcomes in one place
- **Type Safety**: Leverages the type system to ensure all cases are handled

## Code Examples

### Write Side Implementation


### Read Side Implementation

```csharp
// Read model representing a person and their friend
public record PersonFriendRM(Guid PersonId, string Person, string Friend)
{
    // Event handler that updates the read model when FriendChanged event occurs
    public class PersonFriendRMRepository : HerrGeneral.ReadSide.IEventHandler<FriendChanged>
    {
        private readonly IDatabase _database;

        public PersonFriendRMRepository(IDatabase database)
        {
            _database = database;
        }

        public Task Handle(FriendChanged @event, CancellationToken cancellationToken)
        {
            // Update the read model when a friend is changed
            return _database.UpdatePersonFriend(@event.PersonId, @event.NewFriendName);
        }
    }    
}
```

## License

Herr General is released under the [MIT License](LICENSE).
