using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record CreateBankCard(Guid AccountId, string AccountNumber, string CardholderName, CardType CardType) : Create<BankCard>
{
    /// <summary>
    /// Handler for creating new bank cards linked to customer accounts
    /// </summary>
    public class Handler : ICreateHandler<BankCard, CreateBankCard>
    {
        /// <summary>
        /// Creates a new bank card aggregate
        /// </summary>
        /// <param name="command">The create command containing card details</param>
        /// <param name="aggregateId">The unique identifier for the new card</param>
        /// <returns>New bank card aggregate</returns>
        public BankCard Handle(CreateBankCard command, Guid aggregateId) =>
            new(aggregateId, command.AccountId, command.AccountNumber, command.CardholderName, command.CardType, command.Id);
    }
}