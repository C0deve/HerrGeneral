using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

namespace HerrGeneral.SampleApplication.Bank.ReadModel;

public record AccountProjectionItem(
    Guid Id,
    string AccountNumber,
    string OwnerName,
    decimal Balance,
    bool IsActive,
    DateTime CreatedAt,
    DateTime LastTransactionDate,
    int TransactionCount,
    List<string> AssociatedCards); // Card numbers linked to this account

/// <summary>
/// Read-side handler for maintaining account projections
/// </summary>
public class AccountProjection : Projection<AccountProjectionItem>,
    ReadSide.IEventHandler<AccountCreated>,
    ReadSide.IEventHandler<MoneyDeposited>,
    ReadSide.IEventHandler<MoneyWithdrawn>,
    ReadSide.IEventHandler<BankCardCreated>

{
    public void Handle(AccountCreated @event)
    {
        var projection = new AccountProjectionItem(
            Id: @event.AggregateId,
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
            item => item.Id == @event.AggregateId,
            item => item with
            {
                Balance = @event.Balance,
                LastTransactionDate = @event.DateTimeEventOccurred,
                TransactionCount = item.TransactionCount + 1
            });

    public void Handle(MoneyWithdrawn @event) =>
        Update(
            item => item.Id == @event.AggregateId,
            item => item with
            {
                Balance = @event.Balance,
                LastTransactionDate = @event.DateTimeEventOccurred,
                TransactionCount = item.TransactionCount + 1
            });

    public void Handle(BankCardCreated @event) =>
        Update(
            item => item.Id == @event.AccountId,
            item =>
            {
                item.AssociatedCards.Add(@event.CardNumber);
                return item;
            });
}