using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.InternalHandler;

/// <summary>
/// Write-side handler for automatic card blocking on suspicious activity
/// </summary>
public class CardBlockedHandler(INotificationService notification, ISecurityService security)
    : IVoidDomainEventHandler<BankCardBlocked>
{
    public void Handle(BankCardBlocked @event)
    {
        // Notify customer about card blocking
        notification.SendCardBlockedNotification(@event.CardNumber, @event.Reason);

        // Log security event
        security.LogCardBlockingEvent(@event.CardNumber, @event.Reason);
    }
}