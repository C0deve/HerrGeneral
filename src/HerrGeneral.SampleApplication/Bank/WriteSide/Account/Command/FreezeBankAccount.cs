using HerrGeneral.DDD;
using HerrGeneral.WriteSide;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;

/// <summary>
/// Command demonstrating explicit attribute-based partition locking using [AggregateLockKey<BankAccount>].
/// </summary>
public record FreezeBankAccount(AccountNumber AggregateId, string Reason) : Change<BankAccount, AccountNumber>(AggregateId)
{
    /// <summary>
    /// Handler for freezing bank accounts.
    /// </summary>
    public class Handler : IChangeHandler<BankAccount, FreezeBankAccount, AccountNumber>
    {
        public BankAccount Handle(BankAccount aggregate, FreezeBankAccount command) =>
            aggregate.Freeze(command.Reason, command.Id);
    }
}
