namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Exception;

/// <summary>
/// Exception thrown when there are insufficient funds for a withdrawal
/// </summary>
public class InsufficientFundsException(string accountNumber, decimal requestedAmount, decimal availableBalance)
    : DomainException($"Insufficient funds in account {accountNumber}. Requested: {requestedAmount:C}, Available: {availableBalance:C}")
{
    public string AccountNumber => accountNumber;
    public decimal RequestedAmount => requestedAmount;
    public decimal AvailableBalance => availableBalance;
}
