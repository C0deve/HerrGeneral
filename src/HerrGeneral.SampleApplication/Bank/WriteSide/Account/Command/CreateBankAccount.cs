using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;

public record CreateBankAccount(string AccountNumber, string OwnerName, decimal InitialDeposit) : Create<BankAccount>
{
    /// <summary>
    /// Handler for creating new bank accounts
    /// </summary>
    public class Handler : ICreateHandler<BankAccount, CreateBankAccount>
    {
        /// <summary>
        /// Creates a new bank account aggregate
        /// </summary>
        /// <param name="command">The create command containing account details</param>
        /// <param name="aggregateId">The unique identifier for the new account</param>
        /// <returns>New bank account aggregate</returns>
        public BankAccount Handle(CreateBankAccount command, Guid aggregateId) =>
            new(aggregateId, command.AccountNumber, command.OwnerName, command.InitialDeposit, command.Id);
    }
}