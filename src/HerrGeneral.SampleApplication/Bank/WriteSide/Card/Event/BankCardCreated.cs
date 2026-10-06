namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

using HerrGeneral.SampleApplication.Bank.WriteSide.Account;

public record BankCardCreated(
    AccountNumber AccountNumber,
    CardNumber CardNumber,
    string CardholderName,
    CardType CardType,
    Guid SourceCommandId,
    CardNumber AggregateId) 
    : DomainEvent<BankCard, CardNumber>(SourceCommandId, AggregateId);