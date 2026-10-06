namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to perform operations on an inactive card
/// </summary>
public class InactiveCardException(CardNumber cardNumber, string operation)
    : DomainException($"Cannot {operation} with inactive card {cardNumber}")
{
    public CardNumber CardNumber => cardNumber;
    public string Operation => operation;
}
