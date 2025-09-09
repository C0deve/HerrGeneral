namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Exception;

/// <summary>
/// Exception thrown when setting an invalid daily limit
/// </summary>
public class InvalidDailyLimitException(decimal limit, string reason)
    : DomainException($"Invalid daily limit {limit:C}: {reason}")
{
    public decimal Limit => limit;
    public string Reason => reason;
}
