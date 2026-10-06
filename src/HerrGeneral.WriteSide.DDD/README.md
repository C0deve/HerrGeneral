# HerrGeneral.WriteSide.DDD

## Overview

HerrGeneral.WriteSide.DDD provides interfaces, base classes, and attributes for writing domain logic in a DDD style with full support for generic aggregate keys (`TKey : notnull`).

Key elements:
- **Base classes & Abstractions**:
  - Aggregates with integrated domain events (`IAggregate<TKey>`, `Aggregate<TAggregate, TKey>`, default `Aggregate<TAggregate>` with `Guid`)
  - Creation commands (`Create<TAggregate, TKey>`, default `Create<TAggregate>` with `Guid`)
  - Change commands (`Change<TAggregate, TKey>`, default `Change<TAggregate>` with `Guid`), automatically keyed with `(typeof(TAggregate).FullName, (object)AggregateId)`
  - Domain events (`IDomainEvent<TAggregate, TKey>`, default `IDomainEvent<TAggregate>` with `Guid`)
  - Aggregate repositories (`IAggregateRepository<TAggregate, TKey>`, default `IAggregateRepository<TAggregate>` with `Guid`)

- **Concurrency & Locking**:
  - `[AggregateLockKey<TAggregate>]` attribute: Strongly-typed lock key constrained to `IAggregate`.
  - Automatic partition locking via `Change<TAggregate, TKey>.Key`.

- **Interfaces**:
  - **Command handlers** for processing commands:
    - `ICreateHandler<out TAggregate, in TCommand>`
    - `IChangeHandler<TAggregate, in TCommand>`
    - `IChangeMultiHandler<out TAggregate, in TCommand>`
  - **Domain event handlers** for processing domain events:
    - `IHandleCrossAggregate<in TEvent, TAggregate>` for cross-aggregate mutations within the active transaction.
    - `IDomainEventHandler<in TEvent>` for processing events and returning changed aggregates.
    - `IVoidDomainEventHandler<in TEvent>` for processing events without returning anything.
    - `ICrossAggregateChangeHandler<in TEvent, TAggregate>` (backward compatibility alias for `IHandleCrossAggregate`).
  - **Command with automatic handler** (Convention-Based Dynamic Command Handling):
    - `INoHandlerCreate<TAggregate>`
    - `INoHandlerChange<TAggregate>`

## NuGet Package

- **[HerrGeneral.WriteSide.DDD](https://www.nuget.org/packages/HerrGeneral.WriteSide.DDD/)** (in the domain layer)

```bash
dotnet add package HerrGeneral.WriteSide.DDD
```

## Generic Aggregate Keys (`TKey : notnull`)

HerrGeneral supports arbitrary aggregate key types (`Guid`, `string`, `int`, `long`, or strongly-typed Value Objects / `record struct`) while preserving full backwards compatibility with `Guid` defaults:

```csharp
// 1. Default Guid-based aggregate
public class Person : Aggregate<Person>
{
    public Person(Guid id, string name) : base(id) { ... }
}

// 2. String-based aggregate (e.g. SKU, code, slug)
public class Product : Aggregate<Product, string>
{
    public Product(string id, string title) : base(id) { ... }
}

// 3. Strongly-typed ID (record struct)
public readonly record struct OrderId(Guid Value);

public class Order : Aggregate<Order, OrderId>
{
    public Order(OrderId id, decimal amount) : base(id) { ... }
}
```

## Command Concurrency & Locking in DDD

Commands targeting the same aggregate instance are automatically serialized to prevent race conditions while allowing concurrent processing across different aggregates.

### 1. Built-in with `Change<TAggregate, TKey>`

When using the `Change<TAggregate, TKey>` (or `Change<TAggregate>`) base class, partitioning is handled automatically and cached:

```csharp
// Guid aggregate
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId);

// String aggregate
public record UpdateProductPrice(string Sku, decimal NewPrice) : Change<Product, string>(Sku);
```

### 2. Explicit with `[AggregateLockKey<TAggregate>]`

For custom commands that do not inherit from `Change<TAggregate, TKey>`, decorate the aggregate identifier property with `[AggregateLockKey<TAggregate>]`:

```csharp
using HerrGeneral.DDD;

public record CancelOrder(
    [property: AggregateLockKey<Order>] string OrderId, 
    string Reason
);
```

This enforces at compile-time that `Order` implements `IAggregate` and scopes the lock to `(typeof(Order).FullName, OrderId)` without cross-type collisions.

## Code Examples

HerrGeneral.WriteSide.DDD offers two approaches to handling commands: explicit handler classes or convention-based dynamic handlers (`INoHandler*`).

### Commands Handlers

#### 1. Creating an Aggregate

**Explicit Handler:**
```csharp
public record CreatePerson(string Name) : Create<Person>
{
    public class Handler : ICreateHandler<Person, CreatePerson>
    {
        public Person Handle(CreatePerson command, Guid aggregateId) => 
            new(aggregateId, command.Name);
    }
}
```

**Convention-Based Handler (`INoHandlerCreate`):**
```csharp
public record CreateProduct(string Title) : Create<Product, string>, 
    INoHandlerCreate<Product>;

public class Product : Aggregate<Product, string>
{
    // Automatically resolved constructor. Supported parameter variants:
    // - (TCommand command, TKey aggregateId)
    // - (TKey aggregateId, TCommand command)
    // - (TKey aggregateId)
    // - (TCommand command)
    public Product(CreateProduct command, string sku) : base(sku)
    {
        Title = command.Title;
    }
}
```

#### 2. Changing Aggregate State

**Explicit Handler:**
```csharp
public record ChangeName(Guid AggregateId, string NewName) : Change<Person>(AggregateId)
{
    public class Handler : IChangeHandler<Person, ChangeName>
    {
        public Person Handle(Person aggregate, ChangeName command)
        { 
            aggregate.ChangeName(command.NewName);
            return aggregate;
        }
    }
}
```

**Convention-Based Handler (`INoHandlerChange`):**
```csharp
public record UpdateProductPrice(string AggregateId, decimal NewPrice) : Change<Product, string>(AggregateId), 
    INoHandlerChange<Product>;

public class Product : Aggregate<Product, string>
{
    // Automatically discovered and invoked by the runtime engine
    public Product Execute(UpdateProductPrice command)
    {
        Price = command.NewPrice;
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
