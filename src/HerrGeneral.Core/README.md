# HerrGeneral.Core

Essential components for application integration and configuration

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


### Mediator

Send your commands with the mediator:
```csharp
var mediator = serviceProvider.getRequiredService<Mediator>;
var result = mediator.Send(new MyCommand());
```

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