namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface INotificationService
{
    void SendSuspiciousActivityAlert(string accountNumber, decimal amount);
    void SendLowBalanceWarning(string accountNumber, decimal balance);
    void SendDepositConfirmation(string accountNumber, decimal amount);
    void SendSuspiciousCardActivityAlert(string cardNumber, decimal amount, string merchantName);
    void SendDailyLimitWarning(string cardNumber, decimal dailySpentTotal);
    void SendCardBlockedNotification(string cardNumber, string reason);
}