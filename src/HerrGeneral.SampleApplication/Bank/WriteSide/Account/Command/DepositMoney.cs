using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;

public record DepositMoney(Guid AggregateId, decimal Amount, string Description) : Change<BankAccount>(AggregateId)
{
    /// <summary>
    /// Handler for depositing money into bank accounts
    /// </summary>
    public class Handler : IChangeHandler<BankAccount, DepositMoney>
    {
        /// <summary>
        /// Processes a money deposit operation on the bank account
        /// </summary>
        /// <param name="aggregate">The bank account aggregate</param>
        /// <param name="command">The deposit command containing amount and description</param>
        /// <returns>Updated bank account aggregate</returns>
        public BankAccount Handle(BankAccount aggregate, DepositMoney command) =>
            aggregate.Deposit(command.Amount, command.Description, command.Id);
    }
}