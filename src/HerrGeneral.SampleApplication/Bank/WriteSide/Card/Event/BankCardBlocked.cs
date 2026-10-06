namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record BankCardBlocked(CardNumber CardNumber, string Reason, Guid SourceCommandId, CardNumber AggregateId) 
    : DomainEvent<BankCard, CardNumber>(SourceCommandId, AggregateId);