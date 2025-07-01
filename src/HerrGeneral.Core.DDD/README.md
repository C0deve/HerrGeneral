# HerrGeneral.Core.DDD

## Overview

HerrGeneral.Core.DDD is the glue between HerrGeneral.Core and HerrGeneral.WriteSide.DDD
Provide extension methods to register handlers comming from HerrGeneral.WriteSide.DDD

## NuGet Packages

- **[HerrGeneral.Core.DDD](https://www.nuget.org/packages/HerrGeneral.Core.DDD/)**: Support for dynamic command handlers and DDD patterns

## Registration

```csharp
// Register write side assembly and namespace for command and domain event handlers
services.RegisterDDDHandlers(typeof(CreatePerson).Assembly);

// Optional: Enable convention-based dynamic handlers for reduced boilerplate
services.RegisterDynamicHandlers(typeof(CreatePerson).Assembly); 
```

## Prerequisite

#### Install Core Infrastructure
- 
- **[HerrGeneral.Core](https://www.nuget.org/packages/HerrGeneral.Core/)**: Essential components for application integration and configuration

### Register HerrGeneral

```csharp
// Add Herr General to your service collection
services.UseHerrGeneral(configuration =>
        // Register read side assembly and namespace for read model event handlers
        .UseReadSideAssembly(typeof(PersonFriendRM).Assembly, typeof(PersonFriendRM).Namespace!)); 
```

