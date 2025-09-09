using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

namespace HerrGeneral.SampleApplication.Bank.ReadModel;

public record BankCardProjectionItem(
    Guid Id,
    Guid AccountId,
    string CardNumber = "",
    string CardholderName = "",
    string CardType = "", // "Debit", "Credit"
    bool IsActive = false,
    DateTime CreatedAt = default,
    DateTime? BlockedAt = null,
    string? BlockReason = null,
    decimal DailyLimit = 0,
    decimal DailySpent = 0,
    int TransactionCount = 0,
    DateTime? LastTransactionDate = null);

public class BankCardProjection : Projection<BankCardProjectionItem>,
    ReadSide.IEventHandler<BankCardCreated>,
    ReadSide.IEventHandler<CardPaymentProcessed>,
    ReadSide.IEventHandler<BankCardBlocked>,
    ReadSide.IEventHandler<BankCardUnblocked>,
    ReadSide.IEventHandler<CardDailyLimitUpdated>
{
    public void Handle(BankCardCreated @event) =>
        Add(
            new BankCardProjectionItem(
                Id: @event.AggregateId,
                AccountId: @event.AccountId,
                CardNumber: @event.CardNumber,
                CardholderName: @event.CardholderName,
                CardType: @event.CardType.ToString(),
                IsActive: true,
                CreatedAt: @event.DateTimeEventOccurred,
                DailySpent: 0,
                TransactionCount: 0));

    public void Handle(CardPaymentProcessed @event) =>
        Update(
            item => item.Id == @event.AggregateId,
            item => item with
            {
                DailySpent = @event.DailySpentTotal,
                TransactionCount = item.TransactionCount + 1,
                LastTransactionDate = @event.DateTimeEventOccurred
            });

    public void Handle(BankCardBlocked @event) =>
        Update(
            item => item.Id == @event.AggregateId,
            item => item with
            {
                IsActive = false,
                BlockedAt = @event.DateTimeEventOccurred,
                BlockReason = @event.Reason
            });

    public void Handle(BankCardUnblocked @event) =>
        Update(
            item => item.Id == @event.AggregateId,
            item => item with
            {
                IsActive = true,
                BlockedAt = null,
                BlockReason = null
            });

    public void Handle(CardDailyLimitUpdated @event) =>
        Update(
            item => item.Id == @event.AggregateId,
            item => item with
            {
                DailyLimit = @event.NewLimit
            });
}