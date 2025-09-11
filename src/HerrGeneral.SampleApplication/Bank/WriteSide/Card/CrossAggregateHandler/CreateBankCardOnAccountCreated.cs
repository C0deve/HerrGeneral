using HerrGeneral.SampleApplication.Bank.WriteSide.Account.Event;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.CrossAggregateHandler;

/// <summary>
/// Handles the creation of a bank card when a new account is created.
/// </summary>
/// <remarks>
/// This class listens for the <see cref="AccountCreated"/> event and performs the following actions:
/// - Creates a new <see cref="BankCard"/> instance based on the details of the event.
/// - Saves the newly created bank card to the provided repository.
/// </remarks>
/// <param name="bankCardRepository">
/// Repository for saving the created bank card aggregate.
/// </param>
/// <seealso cref="AccountCreated"/>
/// <seealso cref="HerrGeneral.WriteSide.DDD.IAggregateRepository{T}"/>
/// <seealso cref="BankCard"/>
public class CreateBankCardOnAccountCreated(IAggregateRepository<BankCard> bankCardRepository) : IDomainEventHandler<AccountCreated, BankCard>
{
    public IEnumerable<BankCard> Handle(AccountCreated @event)
    {
        yield return new BankCard(Guid.NewGuid(),
            @event.AggregateId,
            @event.AccountNumber,
            @event.OwnerName,
            CardType.Credit,
            @event.SourceCommandId);
    }
}

