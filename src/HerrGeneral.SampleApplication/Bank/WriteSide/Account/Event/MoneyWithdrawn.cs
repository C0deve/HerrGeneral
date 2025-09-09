namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record MoneyWithdrawn(string AccountNumber, decimal Amount, decimal Balance, Guid SourceCommandId, Guid AggregateId) : DomainEvent<BankAccount>(SourceCommandId, AggregateId);