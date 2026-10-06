using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record UpdateCardDailyLimit(CardNumber AggregateId, decimal NewLimit) : Change<BankCard, CardNumber>(AggregateId)
{
    /// <summary>
    /// Handler for updating bank card daily spending limits
    /// </summary>
    public class Handler : IChangeHandler<BankCard, UpdateCardDailyLimit, CardNumber>
    {
        /// <summary>
        /// Updates the daily spending limit for the bank card
        /// </summary>
        /// <param name="aggregate">The bank card aggregate to modify</param>
        /// <param name="command">The update limit command containing the new limit</param>
        /// <returns>Updated bank card aggregate</returns>
        public BankCard Handle(BankCard aggregate, UpdateCardDailyLimit command) =>
            aggregate.UpdateDailyLimit(command.NewLimit, command.Id);
    }
}