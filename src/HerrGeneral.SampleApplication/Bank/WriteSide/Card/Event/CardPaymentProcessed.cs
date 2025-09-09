namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record CardPaymentProcessed(
    Guid AccountId,
    string AccountNumber,
    string CardNumber,
    decimal Amount,
    string MerchantName,
    decimal DailySpentTotal,
    Guid SourceCommandId,
    Guid AggregateId) 
    : DomainEvent<BankCard>(SourceCommandId, AggregateId);