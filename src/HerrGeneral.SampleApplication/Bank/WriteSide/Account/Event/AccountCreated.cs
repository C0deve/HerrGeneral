namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record AccountCreated(string AccountNumber, string OwnerName, decimal InitialDeposit, Guid SourceCommandId, Guid AggregateId) : DomainEvent<BankAccount>(SourceCommandId, AggregateId);