using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record UnblockBankCard(Guid AggregateId) : Change<BankCard>(AggregateId)
{
    /// <summary>
    /// Handler for unblocking bank cards with validation and business logic
    /// </summary>
    public class Handler : IChangeHandler<BankCard, UnblockBankCard>
    {
        /// <summary>
        /// Unblocks the bank card
        /// </summary>
        /// <param name="aggregate">The bank card aggregate to modify</param>
        /// <param name="command">The unblock command</param>
        /// <returns>Updated bank card aggregate</returns>
        public BankCard Handle(BankCard aggregate, UnblockBankCard command) =>
            aggregate.UnblockCard(command.Id);
    }
}