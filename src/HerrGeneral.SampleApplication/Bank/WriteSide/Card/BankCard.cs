using System.Collections.ObjectModel;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card;

/// <summary>
/// Bank Card aggregate representing debit/credit cards linked to bank accounts
/// </summary>
public class BankCard : Aggregate<BankCard>
{
    private readonly List<CardTransaction> _transactions = [];

    public BankCard(Guid id, Guid accountId, string accountNumber, string cardholderName, CardType cardType, Guid commandId)
        : base(id)
    {
        AccountId = accountId;
        AccountNumber = accountNumber;
        CardholderName = cardholderName;
        CardType = cardType;
        CardNumber = GenerateCardNumber();
        ExpiryDate = DateTime.Now.AddYears(3);
        IsActive = true;
        DailyLimit = cardType == CardType.Credit ? 5000m : 1000m;
        DailySpent = 0m;

        Emit(new BankCardCreated(AccountId, CardNumber, CardholderName, cardType, commandId, Id));
    }

    public Guid AccountId { get; }
    public string AccountNumber { get; }
    public string CardholderName { get; }
    public string CardNumber { get; }
    public CardType CardType { get; }
    public DateTime ExpiryDate { get; }
    public bool IsActive { get; private set; }
    public decimal DailyLimit { get; private set; }
    public decimal DailySpent { get; private set; }
    public ReadOnlyCollection<CardTransaction> Transactions => _transactions.AsReadOnly();

    public BankCard ProcessPayment(decimal amount, string merchantName, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveCardException(CardNumber, "process payment");
        if (DateTime.Now > ExpiryDate)
            throw new ExpiredCardException(CardNumber, ExpiryDate);
        if (amount <= 0)
            throw new InvalidAmountException(amount, "Payment");
        if (DailySpent + amount > DailyLimit)
            throw new DailyLimitExceededException(CardNumber, amount, DailySpent, DailyLimit);

        DailySpent += amount;
        var transaction = new CardTransaction(amount, merchantName, DateTime.Now);
        _transactions.Add(transaction);

        return Emit(new CardPaymentProcessed(
            AccountId,
            AccountNumber,
            CardNumber,
            amount,
            merchantName,
            DailySpent,
            commandId,
            Id));
    }

    public BankCard BlockCard(string reason, Guid commandId)
    {
        if (!IsActive)
            throw new CardAlreadyBlockedException(CardNumber);

        IsActive = false;
        return Emit(new BankCardBlocked(CardNumber, reason, commandId, Id));
    }

    public BankCard UnblockCard(Guid commandId)
    {
        if (IsActive)
            throw new CardAlreadyActiveException(CardNumber);
        if (DateTime.Now > ExpiryDate)
            throw new ExpiredCardException(CardNumber, ExpiryDate);

        IsActive = true;
        return Emit(new BankCardUnblocked(CardNumber, commandId, Id));
    }

    public BankCard UpdateDailyLimit(decimal newLimit, Guid commandId)
    {
        if (newLimit <= 0)
            throw new InvalidDailyLimitException(newLimit, "Daily limit must be positive");
        if (newLimit > 10000m)
            throw new InvalidDailyLimitException(newLimit, "Daily limit cannot exceed 10,000");

        var oldLimit = DailyLimit;
        DailyLimit = newLimit;

        return Emit(new CardDailyLimitUpdated(CardNumber, oldLimit, newLimit, commandId, Id));
    }

    public BankCard ResetDailySpent()
    {
        DailySpent = 0m;
        return this;
    }

    private static string GenerateCardNumber()
    {
        // Simplified card number generation
        var random = new Random();
        return $"4532-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}";
    }
}