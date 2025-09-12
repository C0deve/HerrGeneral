// Agrégat Account (parfait pour démontrer les règles métier)

using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;

// Events

// Commandes
public record WithdrawMoney(Guid AggregateId, decimal Amount, string Description) : Change<BankAccount>(AggregateId)
{
    /// <summary>
    /// Handler for withdrawing money from bank accounts
    /// </summary>
    public class Handler : IChangeHandler<BankAccount, WithdrawMoney>
    {
        /// <summary>
        /// Processes a money withdrawal operation on the bank account
        /// </summary>
        /// <param name="aggregate">The bank account aggregate</param>
        /// <param name="command">The withdrawal command containing amount and description</param>
        /// <returns>Updated bank account aggregate</returns>
        public BankAccount Handle(BankAccount aggregate, WithdrawMoney command) =>
            aggregate.Withdraw(command.Amount, command.Description, command.Id);
    }
}