namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record AccountFrozen(AccountNumber AccountNumber, string Reason, Guid SourceCommandId, AccountNumber AggregateId) 
    : DomainEvent<BankAccount, AccountNumber>(SourceCommandId, AggregateId);
