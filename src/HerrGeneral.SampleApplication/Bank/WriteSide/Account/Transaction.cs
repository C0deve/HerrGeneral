namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account;

public record Transaction(TransactionType Deposit, decimal Amount, string Description, DateTime Now);