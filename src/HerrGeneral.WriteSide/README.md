# HerrGeneral.WriteSide

## Overview

(Optional)

Interfaces and attributes for Herr General WriteSide:
- `ICommandHandler` and `IEventHandler` for strong-typed handler mapping without reflection.
- `[LockKey<TAggregate>]` and `IKeyedCommand` for command concurrency control and partition-based locking.

Pros: no reflection to resolve handlers, compile-time safety for concurrency keys  
Cons: adds a dependency on HerrGeneral in the write model

## NuGet Packages

- **[HerrGeneral.WriteSide](https://www.nuget.org/packages/HerrGeneral.WriteSide/)**

```bash
dotnet add package HerrGeneral.WriteSide
```

## Command Concurrency & Locking

HerrGeneral provides partition-based concurrency control to serialize commands targeting the same entity while executing independent commands concurrently.

### 1. Attribute-Based Locking (Recommended)

Use `[LockKey<TAggregate>]` on the identifier property of your command. This is the simplest, most concise, and type-safe approach for positional records:

```csharp
using HerrGeneral.WriteSide;

public record CancelOrder(
    [property: LockKey<Order>] Guid OrderId, 
    string Reason
);
```

You can also use the non-generic `[LockKey(typeof(Order))]`:

```csharp
public record UpdateAddress(
    [property: LockKey(typeof(Order))] Guid OrderId, 
    string Street, 
    string City
);
```

**Benefits:**
- **Zero boilerplate**: Single attribute directly on the record parameter.
- **Automatic key qualification**: The lock is scoped to `(typeof(Order), OrderId)`, preventing lock collisions between different aggregate types with identical IDs.

### 2. Interface-Based Locking (Advanced / Extension Point)

For complex, dynamic, or composite keys (e.g., multi-tenancy), implement `IKeyedCommand`:

```csharp
using HerrGeneral.WriteSide;

public record TransferFunds(
    Guid TenantId, 
    Guid SourceAccountId, 
    decimal Amount
) : IKeyedCommand
{
    // Composite lock key
    public object Key => (TenantId, typeof(Account), SourceAccountId);
}
```
