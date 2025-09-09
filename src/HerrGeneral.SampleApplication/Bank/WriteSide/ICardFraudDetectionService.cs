namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface ICardFraudDetectionService
{
    bool IsSuspiciousCardPayment(string cardNumber, decimal amount, string merchantName);
}