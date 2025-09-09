namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to perform operations on an inactive card
/// </summary>
public class InactiveCardException(string cardNumber, string operation)
    : DomainException($"Cannot {operation} with inactive card {cardNumber}")
{
    public string CardNumber => cardNumber;
    public string Operation => operation;
}
