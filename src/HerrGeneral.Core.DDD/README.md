# HerrGeneral.Core.DDD

## Overview

HerrGeneral.Core.DDD provide methods to register handlers coming from HerrGeneral.WriteSide.DDD
and send commands using mediator with fluent syntax.

## NuGet Packages

- **[HerrGeneral.Core.DDD](https://www.nuget.org/packages/HerrGeneral.Core.DDD/)** (in the infrastructure):  
  The engine of HerrGeneral framework. Needed for adding HerrGeneral to the service container.

## Register HerrGeneral

```csharp
using HerrGeneral.DDD;

// ...
    
services
    .AddHerrGeneral(configuration => configuration
        // Register command handlers and domain event handlers from your specified assembly
        .ScanWriteSideOn(<Your write side assembly>, "Optional parent namespace filter"))
        // Optionally register an aggregate factory needed by INoHandlerCreate<Person> 
        .AddTransient<IAggregateFactory<Person>, DefaultAggregateFactory<Person>>();
```


## Send a command using mediator with fluent syntax

```csharp
using HerrGeneral;

var mediator = serviceProvider.GetRequiredService<Mediator>();

// Creation command returns a Result<Guid>
var result = mediator
    .Send(new CreatePerson(new Person("John", "Doe")))
    .Then(id => Console.WriteLine($"Person with id {id} created"));

//Change command returns a Result<Unit>
var result = await mediator
    .Send(new ChangePersonName(personId, "Martin"))
    .Then(() => Console.WriteLine($"Person with id {personId} has been changed"));
```

## More information

**[HerrGeneral on Github](https://github.com/C0deve/HerrGeneral)**  