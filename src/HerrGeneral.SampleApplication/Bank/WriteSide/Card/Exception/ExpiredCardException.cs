namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when attempting to use an expired card
/// </summary>
public class ExpiredCardException(string cardNumber, DateTime expiryDate)
    : DomainException($"Card {cardNumber} has expired on {expiryDate:MM/yyyy}")
{
    public string CardNumber => cardNumber;
    public DateTime ExpiryDate => expiryDate;
}
