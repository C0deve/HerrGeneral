using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record BlockBankCard(Guid AggregateId, string Reason) : Change<BankCard>(AggregateId)
{
    /// <summary>
    /// Handler for blocking bank cards with validation and business logic
    /// </summary>
    public class Handler : IChangeHandler<BankCard, BlockBankCard>
    {
        /// <summary>
        /// Blocks the bank card with the specified reason
        /// </summary>
        /// <param name="aggregate">The bank card aggregate to modify</param>
        /// <param name="command">The block command containing the reason</param>
        /// <returns>Updated bank card aggregate</returns>
        public BankCard Handle(BankCard aggregate, BlockBankCard command) =>
            aggregate.BlockCard(command.Reason, command.Id);
    }
}