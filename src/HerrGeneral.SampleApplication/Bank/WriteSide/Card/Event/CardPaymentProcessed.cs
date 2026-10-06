namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

using HerrGeneral.SampleApplication.Bank.WriteSide.Account;

public record CardPaymentProcessed(
    AccountNumber AccountNumber,
    CardNumber CardNumber,
    decimal Amount,
    string MerchantName,
    decimal DailySpentTotal,
    Guid SourceCommandId,
    CardNumber AggregateId) 
    : DomainEvent<BankCard, CardNumber>(SourceCommandId, AggregateId);