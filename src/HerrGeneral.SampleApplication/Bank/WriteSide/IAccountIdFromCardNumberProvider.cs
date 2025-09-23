namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface IAccountIdFromCardNumberProvider  
{
    Guid GetFromCardNumber(string eventCardNumber);
}