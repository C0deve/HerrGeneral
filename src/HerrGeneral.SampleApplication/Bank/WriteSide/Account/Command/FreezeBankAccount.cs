using HerrGeneral.DDD;
using HerrGeneral.WriteSide;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account.Command;

/// <summary>
/// Command demonstrating explicit attribute-based partition locking using [AggregateLockKey<BankAccount>].
/// </summary>
public record FreezeBankAccount(
    [property: AggregateLockKey<BankAccount>] Guid AggregateId,
    string Reason
)
{
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Handler for freezing bank accounts.
    /// </summary>
    public class Handler(IMyAggregateRepository<BankAccount> repository) : ICommandHandler<FreezeBankAccount, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(FreezeBankAccount command)
        {
            var account = repository.Get(command.AggregateId);
            account.Freeze(command.Reason, command.Id);
            repository.Save(account);
            return (account.NewEvents, Unit.Default);
        }
    }
}
