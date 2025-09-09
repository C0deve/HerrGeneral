using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.Command;

public record ProcessCardPayment(Guid AggregateId, decimal Amount, string MerchantName) : Change<BankCard>(AggregateId)
{
    /// <summary>
    /// Handler for processing card payments with fraud detection and validation
    /// </summary>
    public class Handler : IChangeHandler<BankCard, ProcessCardPayment>
    {
        /// <summary>
        /// Processes a payment transaction on the bank card
        /// </summary>
        /// <param name="aggregate">The bank card aggregate to modify</param>
        /// <param name="command">The payment command containing amount and merchant details</param>
        /// <returns>Updated bank card aggregate with the processed transaction</returns>
        public BankCard Handle(BankCard aggregate, ProcessCardPayment command) =>
            aggregate.ProcessPayment(command.Amount, command.MerchantName, command.Id);
    }
}