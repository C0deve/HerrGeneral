namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface ISecurityService
{
    void LogCardBlockingEvent(string cardNumber, string reason);
}