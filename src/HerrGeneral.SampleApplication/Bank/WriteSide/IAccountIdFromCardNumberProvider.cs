namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface IAccountIdFromCardNumberProvider  
{
    Account.AccountNumber GetFromCardNumber(Card.CardNumber eventCardNumber);
}