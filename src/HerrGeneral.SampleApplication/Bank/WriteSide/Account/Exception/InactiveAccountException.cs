namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Exception;

/// <summary>
/// Exception thrown when attempting to perform operations on an inactive account
/// </summary>
public class InactiveAccountException(string accountNumber, string operation)  
    : DomainException($"Cannot {operation} on inactive account {accountNumber}")
{
    public string AccountNumber => accountNumber;
    public string Operation => operation;
}
