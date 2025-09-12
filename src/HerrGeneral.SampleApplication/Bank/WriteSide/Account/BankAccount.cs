using System.Collections.ObjectModel;
using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Exception;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account;

public sealed class BankAccount : Aggregate<BankAccount>
{
    private readonly List<Transaction> _transactions = [];
    
    public BankAccount(Guid id, string accountNumber, string ownerName, decimal initialDeposit, Guid commandId) : base(id)
    {
        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Balance = initialDeposit;
        IsActive = true;
        
        _transactions.Add(new Transaction(TransactionType.Deposit, initialDeposit, "Initial deposit", DateTime.Now));
        
        Emit(new AccountCreated(AccountNumber, OwnerName, initialDeposit, commandId, Id));
    }
    
    public string AccountNumber { get; }
    public string OwnerName { get; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public ReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    
    public BankAccount Deposit(decimal amount, string description, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveAccountException(AccountNumber, "deposit");
        if (amount <= 0)
            throw new InvalidAmountException(amount, "Deposit");

        Balance += amount;
        _transactions.Add(new Transaction(TransactionType.Deposit, amount, description, DateTime.Now));

        return Emit(new MoneyDeposited(AccountNumber, amount, Balance, commandId, Id));
    }

    public BankAccount Withdraw(decimal amount, string description, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveAccountException(AccountNumber, "withdraw");
        if (amount <= 0)
            throw new InvalidAmountException(amount, "Withdrawal");
        if (Balance < amount)
            throw new InsufficientFundsException(AccountNumber, amount, Balance);
            
        Balance -= amount;
        _transactions.Add(new Transaction(TransactionType.Withdrawal, -amount, description, DateTime.Now));
        
        return Emit(new MoneyWithdrawn(AccountNumber, amount, Balance, commandId, Id));
    }
}