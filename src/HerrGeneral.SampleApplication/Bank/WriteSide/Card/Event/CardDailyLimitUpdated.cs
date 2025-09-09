namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record CardDailyLimitUpdated(string CardNumber, decimal OldLimit, decimal NewLimit, Guid SourceCommandId, Guid AggregateId) 
    : DomainEvent<BankCard>(SourceCommandId, AggregateId);