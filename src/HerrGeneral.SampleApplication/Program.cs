using HerrGeneral.Core;
using HerrGeneral.Core.DDD;
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
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole())
    .AddSingleton<IAggregateRepository<BankAccount>, Repository<BankAccount>>()
    .AddSingleton<IAggregateRepository<BankCard>, Repository<BankCard>>()
    .AddSingleton<HerrGeneral.WriteSide.DDD.IAggregateRepository<BankAccount>>(provider => provider.GetRequiredService<IAggregateRepository<BankAccount>>())
    .AddSingleton<HerrGeneral.WriteSide.DDD.IAggregateRepository<BankCard>>(provider => provider.GetRequiredService<IAggregateRepository<BankCard>>())
    .AddSingleton<BankCardProjection>()
    .AddSingleton<AccountProjection>()
    .AddSingleton<TransactionHistory>()
    .AddHerrGeneral(configuration =>
        configuration
            .ScanReadSideOn(typeof(AccountProjection).Assembly, "HerrGeneral.SampleApplication.Bank.ReadModel")
            .ScanWriteSideOn(typeof(BankCard).Assembly, "HerrGeneral.SampleApplication.Bank.WriteSide")
    );

var serviceProvider = services.BuildServiceProvider();
Console.WriteLine("Initialization Ok");

var mediator = serviceProvider.GetRequiredService<Mediator>();

var accountId = Guid.Empty;

Console.WriteLine("Creating Smith account...");

await mediator
    .Send<Guid>(new CreateBankAccount("AC-3215", "Smith", 1000))
    .Then(id =>
    {
        accountId = id;
        DisplayAccount(serviceProvider, accountId);
        Console.WriteLine("Withdraw 1000 €...");
        return mediator.Send(new DepositMoney(accountId, 1000, "Anniversaire :D"));
    })
    .Then(() =>
    {
        DisplayAccount(serviceProvider, accountId);
        Console.WriteLine("Set Alfred as a friend");
        return mediator.Send(new WithdrawMoney(accountId, 100, "Food"));
    })
    .Match(() => DisplayAccount(serviceProvider, accountId),
        _ => Console.WriteLine("Fail"),
        _ => Console.WriteLine("Fail"));

Console.WriteLine("Transaction history:");
foreach (var transactionHistoryItem in serviceProvider.GetRequiredService<TransactionHistory>().All()) 
    Console.WriteLine($"{transactionHistoryItem.AccountNumber} {transactionHistoryItem.CardNumber} {transactionHistoryItem.Amount} {transactionHistoryItem.Description}");

Console.ReadKey();
return;

void DisplayAccount(IServiceProvider serviceProvider1, Guid guid)
{
    var rm = serviceProvider1.GetRequiredService<AccountProjection>().All().First(x => x.Id == guid);
    Console.WriteLine($"{rm.AccountNumber} {rm.OwnerName} created at {rm.CreatedAt:D} balance is {rm.Balance}.");
    Console.WriteLine($"Card numbers: {string.Join(", ", rm.AssociatedCards)}.");
}