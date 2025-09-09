namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when an invalid amount is provided for a transaction
/// </summary>
public class InvalidAmountException(decimal amount, string operation)
    : DomainException($"{operation} amount must be positive. Provided amount: {amount}")
{
    public decimal Amount => amount;
    public string Operation => operation;
}
