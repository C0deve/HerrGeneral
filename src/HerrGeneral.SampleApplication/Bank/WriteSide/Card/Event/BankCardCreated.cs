namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record BankCardCreated(
    Guid AccountId,
    string CardNumber,
    string CardholderName,
    CardType CardType,
    Guid SourceCommandId,
    Guid AggregateId) 
    : DomainEvent<BankCard>(SourceCommandId, AggregateId);