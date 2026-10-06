namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Exception;

/// <summary>
/// Exception thrown when attempting to perform operations on an inactive account
/// </summary>
public class InactiveAccountException(AccountNumber accountNumber, string operation)  
    : DomainException($"Cannot {operation} on inactive account {accountNumber}")
{
    public AccountNumber AccountNumber => accountNumber;
    public string Operation => operation;
}
