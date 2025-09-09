namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to block an already blocked card
/// </summary>
public class CardAlreadyBlockedException(string cardNumber)
    : DomainException($"Card {cardNumber} is already blocked")
{
    public string CardNumber => cardNumber;
}
