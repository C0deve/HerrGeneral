using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;
using HerrGeneral.WriteSide.DDD;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.CrossAggregateHandler;

/// <summary>
/// Cross-aggregate handler: Debit account balance when card payment is processed
/// </summary>
public class DebitAccountOnCardPayment(
    IAggregateRepository<BankAccount> accountRepository,
    ILogger<DebitAccountOnCardPayment> logger)
    : IEventHandler<CardPaymentProcessed, BankAccount>
{
    public IEnumerable<BankAccount> Handle(CardPaymentProcessed @event)
    {
        // Find the associated bank account (in a real system, you'd have this relationship stored)
        var accounts = accountRepository.FindBySpecification(account => account.AccountNumber == @event.CardNumber[..4]); // Simplified lookup

        var account = accounts.FirstOrDefault();
        if (account == null) yield break;

        // Debit the account balance
        account.Withdraw(@event.Amount, $"Card payment: {@event.MerchantName}", @event.SourceCommandId);

        logger.LogInformation("Debited {Amount} from account {AccountNumber} for card payment at {Merchant}",
            @event.Amount, account.AccountNumber, @event.MerchantName);
        
        yield return account;
    }
}