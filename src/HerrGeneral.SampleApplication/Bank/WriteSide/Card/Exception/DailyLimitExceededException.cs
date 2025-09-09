namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when a transaction would exceed the daily limit
/// </summary>
public class DailyLimitExceededException(string cardNumber, decimal transactionAmount, decimal dailySpent, decimal dailyLimit)
    : DomainException($"Transaction of {transactionAmount:C} on card {cardNumber} would exceed daily limit of {dailyLimit:C}. Already spent: {dailySpent:C}")
{
    public string CardNumber => cardNumber;
    public decimal TransactionAmount => transactionAmount;
    public decimal DailySpent => dailySpent;
    public decimal DailyLimit => dailyLimit;
}
