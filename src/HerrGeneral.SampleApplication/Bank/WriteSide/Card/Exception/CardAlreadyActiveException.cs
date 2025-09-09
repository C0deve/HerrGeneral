namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to unblock an already active card
/// </summary>
public class CardAlreadyActiveException(string cardNumber)
    : DomainException($"Card {cardNumber} is already active")
{
    public string CardNumber => cardNumber;
}
