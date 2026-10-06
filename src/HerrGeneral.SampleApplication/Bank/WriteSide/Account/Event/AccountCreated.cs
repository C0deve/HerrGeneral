namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record AccountCreated(AccountNumber AccountNumber, string OwnerName, decimal InitialDeposit, Guid SourceCommandId, AccountNumber AggregateId) 
    : DomainEvent<BankAccount, AccountNumber>(SourceCommandId, AggregateId);