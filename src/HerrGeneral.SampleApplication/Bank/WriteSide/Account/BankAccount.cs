using System.Collections.ObjectModel;
using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Exception;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account;

public sealed class BankAccount : Aggregate<BankAccount, AccountNumber>
{
    private readonly List<Transaction> _transactions = [];
    
    public BankAccount(AccountNumber accountNumber, string ownerName, decimal initialDeposit, Guid commandId) : base(accountNumber)
    {
        OwnerName = ownerName;
        Balance = initialDeposit;
        IsActive = true;
        
        _transactions.Add(new Transaction(TransactionType.Deposit, initialDeposit, "Initial deposit", DateTime.Now));
        
        Emit(new AccountCreated(Id, OwnerName, initialDeposit, commandId, Id));
    }
    
    public AccountNumber AccountNumber => Id;
    public string OwnerName { get; }
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }
    public ReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    
    public BankAccount Deposit(decimal amount, string description, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveAccountException(Id, "deposit");
        if (amount <= 0)
            throw new InvalidAmountException(amount, "Deposit");

        Balance += amount;
        _transactions.Add(new Transaction(TransactionType.Deposit, amount, description, DateTime.Now));

        return Emit(new MoneyDeposited(Id, amount, Balance, commandId, Id));
    }

    public BankAccount Withdraw(decimal amount, string description, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveAccountException(Id, "withdraw");
        if (amount <= 0)
            throw new InvalidAmountException(amount, "Withdrawal");
        if (Balance < amount)
            throw new InsufficientFundsException(Id, amount, Balance);
            
        Balance -= amount;
        _transactions.Add(new Transaction(TransactionType.Withdrawal, -amount, description, DateTime.Now));
        
        return Emit(new MoneyWithdrawn(Id, amount, Balance, commandId, Id));
    }

    public BankAccount Freeze(string reason, Guid commandId)
    {
        if (!IsActive)
            throw new InactiveAccountException(Id, "freeze");

        IsActive = false;
        return Emit(new AccountFrozen(Id, reason, commandId, Id));
    }
}