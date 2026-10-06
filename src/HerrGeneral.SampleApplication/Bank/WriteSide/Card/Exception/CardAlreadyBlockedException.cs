namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to block an already blocked card
/// </summary>
public class CardAlreadyBlockedException(CardNumber cardNumber)
    : DomainException($"Card {cardNumber} is already blocked")
{
    public CardNumber CardNumber => cardNumber;
}
