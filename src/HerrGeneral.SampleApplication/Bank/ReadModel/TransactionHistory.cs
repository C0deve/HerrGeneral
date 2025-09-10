using HerrGeneral.ReadSide;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

namespace HerrGeneral.SampleApplication.Bank.ReadModel;

/// <summary>
/// Transaction history projection
/// </summary>
public record TransactionHistoryItem(
    Guid Id,
    Guid AccountId,
    string AccountNumber,
    string Type, // "Deposit", "Withdrawal", "Card Payment"
    decimal Amount,
    decimal BalanceAfter,
    DateTime Timestamp,
    string Description,
    string Channel, // "Branch/ATM", "Card", "Online"
    string? MerchantName, // For card payments
    string? CardNumber // For card payments
);

/// <summary>
/// Read-side handler for transaction history
/// </summary>
public class TransactionHistory : Projection<TransactionHistoryItem>,
    IProjectionEventHandler<MoneyDeposited>,
    IProjectionEventHandler<MoneyWithdrawn>,
    IProjectionEventHandler<CardPaymentProcessed>
{
    public void Handle(MoneyDeposited @event) =>
        Add(new TransactionHistoryItem(
            Guid.NewGuid(),
            @event.AggregateId,
            @event.AccountNumber,
            "Deposit",
            @event.Amount,
            @event.Balance,
            @event.DateTimeEventOccurred,
            "Money deposited",
            "Branch/ATM",
            null,
            null
        ));

    public void Handle(MoneyWithdrawn @event) =>
        Add(new TransactionHistoryItem(
            Guid.NewGuid(),
            @event.AggregateId,
            @event.AccountNumber,
            "Withdrawal",
            -@event.Amount,
            @event.Balance,
            @event.DateTimeEventOccurred,
            "Money withdrawn",
            "Branch/ATM",
            null,
            null
        ));

    public void Handle(CardPaymentProcessed @event) =>
        Add(new TransactionHistoryItem(
            Guid.NewGuid(),
            @event.AccountId,
            @event.AccountNumber,
            "Card Payment",
            -@event.Amount,
            0, // Would need account balance
            @event.DateTimeEventOccurred,
            $"Card payment at {@event.MerchantName}",
            "Card",
            @event.MerchantName,
            @event.CardNumber
        ));
}