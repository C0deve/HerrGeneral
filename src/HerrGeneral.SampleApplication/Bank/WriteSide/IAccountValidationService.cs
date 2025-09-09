namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface IAccountValidationService
{
    Task ValidateNewAccount(string accountNumber, string ownerName);
}