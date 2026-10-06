using HerrGeneral.ReadSide;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

namespace HerrGeneral.SampleApplication.Bank.ReadModel;

public record AccountProjectionItem(
    AccountNumber AccountNumber,
    string OwnerName,
    decimal Balance,
    bool IsActive,
    DateTime CreatedAt,
    DateTime LastTransactionDate,
    int TransactionCount,
    List<CardNumber> AssociatedCards); // Card numbers linked to this account

/// <summary>
/// Read-side handler for maintaining account projections
/// </summary>
public class AccountProjection : Projection<AccountProjectionItem>,
    IProjectionEventHandler<AccountCreated>,
    IProjectionEventHandler<MoneyDeposited>,
    IProjectionEventHandler<MoneyWithdrawn>,
    IProjectionEventHandler<BankCardCreated>,
    IProjectionEventHandler<AccountFrozen>
{
    public void Handle(AccountCreated @event)
    {
        var projection = new AccountProjectionItem(
            AccountNumber: @event.AccountNumber,
            OwnerName: @event.OwnerName,
            Balance: @event.InitialDeposit,
            IsActive: true,
            CreatedAt: @event.DateTimeEventOccurred,
            LastTransactionDate: @event.DateTimeEventOccurred,
            TransactionCount: 1,
            AssociatedCards: []);

        Add(projection);
    }

    public void Handle(MoneyDeposited @event) =>
        Update(
            item => item.AccountNumber == @event.AggregateId,
            item => item with
            {
                Balance = @event.Balance,
                LastTransactionDate = @event.DateTimeEventOccurred,
                TransactionCount = item.TransactionCount + 1
            });

    public void Handle(MoneyWithdrawn @event) =>
        Update(
            item => item.AccountNumber == @event.AggregateId,
            item => item with
            {
                Balance = @event.Balance,
                LastTransactionDate = @event.DateTimeEventOccurred,
                TransactionCount = item.TransactionCount + 1
            });

    public void Handle(BankCardCreated @event) =>
        Update(
            item => item.AccountNumber == @event.AccountNumber,
            item =>
            {
                item.AssociatedCards.Add(@event.CardNumber);
                return item;
            });

    public void Handle(AccountFrozen @event) =>
        Update(
            item => item.AccountNumber == @event.AggregateId,
            item => item with
            {
                IsActive = false,
                LastTransactionDate = @event.DateTimeEventOccurred
            });
}