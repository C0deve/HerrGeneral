# HerrGeneral.Testing

Testing utilities and fluent assertions for **HerrGeneral** CQRS applications and unit tests.

## Installation

```bash
dotnet add package HerrGeneral.Testing
```

## Features & Usage

### 1. Fluent Command Dispatch & Result Assertions

Simplify unit and integration testing of mediator commands with extension methods that automatically execute the command and assert success or return the expected value:

```csharp
using HerrGeneral.Testing;

// Dispatch and assert success
await new FreezeBankAccount(accountId).AssertSendFrom(mediator);

// Dispatch creation command and retrieve the resulting value (e.g., Guid ID)
Guid accountId = await new OpenBankAccount("John Doe").AssertSendFrom<Guid>(mediator);
```

### 2. Task & Result Assertions

Assert the outcome of asynchronous command executions directly:

```csharp
using HerrGeneral.Testing;

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
using HerrGeneral.Testing;

services.AddHerrGeneralTestLogger(testOutputHelper, LogLevel.Debug);
```

When enabled, test runs produce a structured hierarchical causal tree in test output:

```text
CMD [OpenBankAccount] (thread #4) ................................. [OK] (8.3ms)
 |
 \--> (cmd) OpenBankAccountHandler (0.5ms)
       |
       |-- (evt) AccountOpened
       |    \--> (wr) CreateDebitCardOnAccountOpened (3.1ms)
       |          \-- (evt) DebitCardCreated
       |
       \== [TX COMMIT] (1.2ms)
       |
       +-- [SYNC PROJECTIONS] (Read-Side)
       |    \-- AccountOpened                ===> AccountSummaryView           (1.1ms)
       |
       \-- [POST TRANSACTION] (Side Effects & Outbox)
            \-- AccountOpened                ===> SendWelcomeEmailSideEffect   (1.9ms)
```

## Compatibility

- Targets `.NET 8.0`, `.NET 9.0`, and `.NET 10.0`.
