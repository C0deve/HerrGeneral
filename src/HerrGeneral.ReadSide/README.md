# HerrGeneral.ReadSide

## Overview

`HerrGeneral.ReadSide` provides lightweight contracts for projection updates and side-effect processing.

### Key Interfaces

1. **`IHandleSyncProjection<in TEvent>`**:
   Synchronous in-transaction projection handler. Runs before `IUnitOfWork.Commit()`. If an exception is thrown, the transaction is rolled back.

   ```csharp
   public class OrderSummaryProjection : IHandleSyncProjection<OrderPlacedEvent>
   {
       public void Handle(OrderPlacedEvent @event)
       {
           // Update database view within the same active transaction
       }
   }
   ```

2. **`IHandlePostProjection<in TEvent>`**:
   Post-transaction eventual consistency projection handler. Runs strictly after `IUnitOfWork.Commit()` succeeds. Exceptions are traced and isolated.

   ```csharp
   public class CustomerAnalyticsProjection : IHandlePostProjection<OrderPlacedEvent>
   {
       public void Handle(OrderPlacedEvent @event)
       {
           // Update external search index or eventual read model
       }
   }
   ```

3. **`IHandleSideEffect<in TEvent>`**:
   Post-transaction side-effect handler for external integrations, notifications, and event broadcasting.

   ```csharp
   public class SendOrderConfirmationEmail : IHandleSideEffect<OrderPlacedEvent>
   {
       public void Handle(OrderPlacedEvent @event)
       {
           // Send confirmation email via SMTP
       }
   }
   ```

4. **`IProjectionEventHandler<in TEvent>`**:
   Retained for backward compatibility. Inherits from `IHandlePostProjection<in TEvent>`.

## NuGet Package

- **[HerrGeneral.ReadSide](https://www.nuget.org/packages/HerrGeneral.ReadSide/)**
