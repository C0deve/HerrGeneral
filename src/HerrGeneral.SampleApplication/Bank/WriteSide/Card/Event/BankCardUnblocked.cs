namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record BankCardUnblocked(CardNumber CardNumber, Guid SourceCommandId, CardNumber AggregateId) 
    : DomainEvent<BankCard, CardNumber>(SourceCommandId, AggregateId);