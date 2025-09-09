namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record BankCardBlocked(string CardNumber, string Reason, Guid SourceCommandId, Guid AggregateId) 
    : DomainEvent<BankCard>(SourceCommandId, AggregateId);