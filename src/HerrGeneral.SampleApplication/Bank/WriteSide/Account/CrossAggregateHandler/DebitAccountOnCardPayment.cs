using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.CrossAggregateHandler;

/// <summary>
/// Cross-aggregate handler: Debit account balance when card payment is processed
/// </summary>
public class DebitAccountOnCardPayment(ILogger<DebitAccountOnCardPayment> logger)
    : ChangesPlanner<BankAccount, AccountNumber>, ICrossAggregateChangeHandler<CardPaymentProcessed, BankAccount, AccountNumber>
{
    public ChangeRequests<BankAccount, AccountNumber> Handle(CardPaymentProcessed @event) =>
        Changes
            .Add(account =>
                {
                    // Debit the account balance
                    account.Withdraw(@event.Amount, $"Card payment: {@event.MerchantName}", @event.SourceCommandId);

                    logger.LogInformation("Debited {Amount} from account {AccountNumber} for card payment at {Merchant}",
                        @event.Amount, account.AccountNumber, @event.MerchantName);

                    return account;
                },
                @event.AccountNumber
            );
}