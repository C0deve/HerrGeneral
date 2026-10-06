namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

public record CardDailyLimitUpdated(CardNumber CardNumber, decimal OldLimit, decimal NewLimit, Guid SourceCommandId, CardNumber AggregateId) 
    : DomainEvent<BankCard, CardNumber>(SourceCommandId, AggregateId);