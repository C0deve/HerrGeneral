using HerrGeneral;
using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.Infrastructure;
using HerrGeneral.SampleApplication.Bank.ReadModel;
using HerrGeneral.SampleApplication.Bank.WriteSide;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection()
    .AddLogging(builder => builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole())
    .AddSingleton<IMyAggregateRepository<BankAccount, AccountNumber>, Repository<BankAccount, AccountNumber>>()
    .AddSingleton<IMyAggregateRepository<BankCard, CardNumber>, Repository<BankCard, CardNumber>>()
    .AddSingleton<IAggregateRepository<BankAccount, AccountNumber>>(provider => provider.GetRequiredService<IMyAggregateRepository<BankAccount, AccountNumber>>())
    .AddSingleton<IAggregateRepository<BankCard, CardNumber>>(provider => provider.GetRequiredService<IMyAggregateRepository<BankCard, CardNumber>>())
    .AddSingleton<BankCardProjection>()
    .AddSingleton<AccountProjection>()
    .AddSingleton<TransactionHistory>()
    .AddHerrGeneral(configuration =>
        configuration
            .ScanReadSideOn(typeof(AccountProjection).Assembly, "HerrGeneral.SampleApplication.Bank.ReadModel")
            .ScanWriteSideOn(typeof(BankCard).Assembly, "HerrGeneral.SampleApplication.Bank.WriteSide")
    );

var serviceProvider = services.BuildServiceProvider();
var mediator = serviceProvider.GetRequiredService<Mediator>();

Console.WriteLine("================================================================================");
Console.WriteLine("               HERRGENERAL - CONCURRENCY & PARTITION LOCKING DEMO               ");
Console.WriteLine("================================================================================\n");

// -----------------------------------------------------------------------------------------
// 1. Basic Flow with Change<TAggregate, TKey> and Strongly-Typed Keys
// -----------------------------------------------------------------------------------------
Console.WriteLine("--- 1. Single Account Sequential Operations ---");

AccountNumber smithAccount = new("AC-1001");
var createSmithResult = await mediator.Send<AccountNumber>(new CreateBankAccount(smithAccount, "Smith", 1000));
createSmithResult.Match(
    id => { Console.WriteLine($"[CREATED] Account Smith created (ID: {id}) with 1000 EUR."); },
    _ => Console.WriteLine("[ERROR] Failed to create account."),
    _ => Console.WriteLine("[ERROR] Failed to create account."));

// Commands inheriting from Change<BankAccount, AccountNumber> are automatically keyed by (typeof(BankAccount), AggregateId)
await mediator.Send(new DepositMoney(smithAccount, 500, "Bonus deposit"));
Console.WriteLine("[DEPOSIT] Deposited 500 EUR (via Change<BankAccount, AccountNumber> automatic lock key).");

await mediator.Send(new WithdrawMoney(smithAccount, 200, "ATM Withdrawal"));
Console.WriteLine("[WITHDRAW] Withdrawn 200 EUR (via Change<BankAccount, AccountNumber> automatic lock key).");

// Explicit attribute locking using [AggregateLockKey<BankAccount>]
await mediator.Send(new FreezeBankAccount(smithAccount, "Suspicious activity detected"));
Console.WriteLine("[FREEZE] Account frozen (via Change<BankAccount, AccountNumber> lock key).");

DisplayAccount(serviceProvider, smithAccount);

// -----------------------------------------------------------------------------------------
// 2. Concurrency Demonstration: Serialized execution on the same aggregate
// -----------------------------------------------------------------------------------------
Console.WriteLine("\n--- 2. Partition Concurrency: 5 Simultaneous Deposits on Same Account (Alice) ---");

AccountNumber aliceAccount = new("AC-2001");
var createAliceResult = await mediator.Send<AccountNumber>(new CreateBankAccount(aliceAccount, "Alice", 100));
createAliceResult.Match(
    id => { Console.WriteLine($"[CREATED] Account Alice created (ID: {id}) with initial balance 100 EUR."); },
    _ => { },
    _ => { });

Console.WriteLine("[CONCURRENCY] Launching 5 parallel deposits of 50 EUR simultaneously with Task.WhenAll...");

var parallelDepositsOnAlice = Enumerable.Range(1, 5).Select(async index =>
{
    var result = await mediator.Send(new DepositMoney(aliceAccount, 50, $"Parallel deposit #{index}"));
    return (Index: index, result.IsSuccess);
});

var aliceDepositResults = await Task.WhenAll(parallelDepositsOnAlice);
Console.WriteLine($"[CONCURRENCY] Completed 5 parallel operations: {aliceDepositResults.Count(r => r.IsSuccess)} succeeded.");
Console.WriteLine("Result: Thanks to partition-based locking, operations were serialized safely without race conditions or lost updates.");
DisplayAccount(serviceProvider, aliceAccount);

// -----------------------------------------------------------------------------------------
// 3. Concurrency Demonstration: Independent aggregates execute in parallel
// -----------------------------------------------------------------------------------------
Console.WriteLine("\n--- 3. Partition Concurrency: Independent Accounts Execute Concurrently ---");

AccountNumber bobAccount = new("AC-3001");
var createBobResult = await mediator.Send<AccountNumber>(new CreateBankAccount(bobAccount, "Bob", 300));
createBobResult.Match(
    id => { Console.WriteLine($"[CREATED] Account Bob created (ID: {id}) with initial balance 300 EUR."); },
    _ => { },
    _ => { });

Console.WriteLine("[CONCURRENCY] Launching simultaneous operations on Alice and Bob in parallel...");

var taskAlice = mediator.Send(new DepositMoney(aliceAccount, 100, "Deposit Alice"));
var taskBob = mediator.Send(new WithdrawMoney(bobAccount, 50, "Withdraw Bob"));

await Task.WhenAll(taskAlice, taskBob);
Console.WriteLine("[CONCURRENCY] Both operations targeting different aggregates executed concurrently without blocking each other.");

DisplayAccount(serviceProvider, aliceAccount);
DisplayAccount(serviceProvider, bobAccount);

// -----------------------------------------------------------------------------------------
// 4. Transaction History Read Model
// -----------------------------------------------------------------------------------------
Console.WriteLine("\n--- 4. Read Model: Transaction History ---");
foreach (var item in serviceProvider.GetRequiredService<TransactionHistory>().All())
{
    Console.WriteLine($"[{item.Timestamp:HH:mm:ss.fff}] Account: {item.AccountNumber,-8} | {item.Type,-12} | {item.Amount,8:N2} EUR | {item.Description}");
}

Console.WriteLine("\n================================================================================");
Console.WriteLine("                              DEMO COMPLETED                                    ");
Console.WriteLine("================================================================================");
return;

void DisplayAccount(IServiceProvider sp, AccountNumber accountNumber)
{
    var rm = sp.GetRequiredService<AccountProjection>().All().First(x => x.AccountNumber == accountNumber);
    Console.WriteLine($" -> [PROJECTION] Account: {rm.AccountNumber} | Owner: {rm.OwnerName,-6} | Active: {rm.IsActive,-5} | Balance: {rm.Balance,8:N2} EUR | Transactions: {rm.TransactionCount}");
}