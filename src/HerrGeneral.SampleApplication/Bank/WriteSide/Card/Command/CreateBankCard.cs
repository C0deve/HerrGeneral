using HerrGeneral.DDD;

using HerrGeneral.SampleApplication.Bank.WriteSide.Account;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record CreateBankCard(CardNumber CardNumber, AccountNumber AccountNumber, string CardholderName, CardType CardType) : Create<BankCard, CardNumber>
{
    /// <summary>
    /// Handler for creating new bank cards linked to customer accounts
    /// </summary>
    public class Handler : ICreateHandler<BankCard, CreateBankCard, CardNumber>
    {
        /// <summary>
        /// Creates a new bank card aggregate
        /// </summary>
        /// <param name="command">The create command containing card details</param>
        /// <param name="aggregateId">The unique identifier for the new card</param>
        /// <returns>New bank card aggregate</returns>
        public BankCard Handle(CreateBankCard command, CardNumber aggregateId) =>
            new(command.CardNumber, command.AccountNumber, command.CardholderName, command.CardType, command.Id);
    }
}