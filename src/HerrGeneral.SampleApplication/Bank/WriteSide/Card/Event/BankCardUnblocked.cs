namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record BankCardUnblocked(string CardNumber, Guid SourceCommandId, Guid AggregateId) 
    : DomainEvent<BankCard>(SourceCommandId, AggregateId);