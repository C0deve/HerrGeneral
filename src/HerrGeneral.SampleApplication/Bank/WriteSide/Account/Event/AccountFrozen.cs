namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record AccountFrozen(string AccountNumber, string Reason, Guid SourceCommandId, Guid AggregateId) : DomainEvent<BankAccount>(SourceCommandId, AggregateId);
