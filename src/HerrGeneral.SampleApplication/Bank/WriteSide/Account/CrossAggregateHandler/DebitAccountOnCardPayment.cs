using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.CrossAggregateHandler;

/// <summary>
/// Cross-aggregate handler: Debit account balance when card payment is processed
/// </summary>
public class DebitAccountOnCardPayment(
    IAccountIdFromCardNumberProvider accountIdProvider,
    ILogger<DebitAccountOnCardPayment> logger)
    : ChangesPlanner<BankAccount>, ICrossAggregateChangeHandler<CardPaymentProcessed, BankAccount>
{
    public ChangeRequests<BankAccount> Handle(CardPaymentProcessed @event) =>
        Changes
            .Add(account =>
                {
                    // Debit the account balance
                    account.Withdraw(@event.Amount, $"Card payment: {@event.MerchantName}", @event.SourceCommandId);

                    logger.LogInformation("Debited {Amount} from account {AccountNumber} for card payment at {Merchant}",
                        @event.Amount, account.AccountNumber, @event.MerchantName);

                    return account;
                },
                accountIdProvider.GetFromCardNumber(@event.CardNumber)
            );
}