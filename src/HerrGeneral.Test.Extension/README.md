# HerrGeneral.Test.Extension

Testing utilities and fluent assertions for **HerrGeneral** CQRS applications and unit tests.

## Installation

```bash
dotnet add package HerrGeneral.Test.Extension
```

## Features & Usage

### 1. Fluent Command Dispatch & Result Assertions

Simplify unit and integration testing of mediator commands with extension methods that automatically execute the command and assert success or return the expected value:

```csharp
using HerrGeneral.Test;

// Dispatch and assert success
await new FreezeBankAccount(accountId).AssertSendFrom(mediator);

// Dispatch creation command and retrieve the resulting value (e.g., Guid ID)
Guid accountId = await new OpenBankAccount("John Doe").AssertSendFrom<Guid>(mediator);
```

### 2. Task & Result Assertions

Assert the outcome of asynchronous command executions directly:

```csharp
using HerrGeneral.Test;

// Assert success and unwrap value
var accountId = await mediator.Send<Guid>(new OpenBankAccount("Alice")).ShouldSuccess();

// Assert success with specific value
await mediator.Send<string>(new GetStatus(accountId)).ShouldSuccessWithValue("Active");

// Assert domain errors (business rule failures)
await mediator.Send(new DebitAccount(accountId, 1000m))
    .ShouldFailWithDomainErrorOfType<InsufficientFundsError>();

// Assert panic exceptions (technical exceptions)
await mediator.Send(new InvalidOperationCommand())
    .ShouldFailWithPanicExceptionOfType<InvalidOperationException>();
```

### 3. xUnit Test Output Logging

Integrate Microsoft.Extensions.Logging with xUnit's `ITestOutputHelper` during test execution:

```csharp
using HerrGeneral.Test;

services.AddHerrGeneralTestLogger(testOutputHelper, LogLevel.Debug);
```

## NuGet Package

- **[HerrGeneral.Test.Extension](https://www.nuget.org/packages/HerrGeneral.Test.Extension/)**
