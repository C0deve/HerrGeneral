namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;

public record MoneyDeposited(AccountNumber AccountNumber, decimal Amount, decimal Balance, Guid SourceCommandId, AccountNumber AggregateId) 
    : DomainEvent<BankAccount, AccountNumber>(SourceCommandId, AggregateId);