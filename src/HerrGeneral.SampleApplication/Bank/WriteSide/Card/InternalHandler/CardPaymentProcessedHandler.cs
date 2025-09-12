using HerrGeneral.DDD;
using HerrGeneral.SampleApplication.Bank.WriteSide.Card.Event;

namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card.InternalHandler;

/// <summary>
/// Write-side handler for card payment fraud detection
/// </summary>
public class CardPaymentProcessedHandler(
    ICardFraudDetectionService fraudDetection,
    INotificationService notification)
    : IVoidDomainEventHandler<CardPaymentProcessed>
{

    public void Handle(CardPaymentProcessed @event)
    {
        // Fraud detection for card payments
        if (fraudDetection.IsSuspiciousCardPayment(@event.CardNumber, @event.Amount, @event.MerchantName)) 
            notification.SendSuspiciousCardActivityAlert(@event.CardNumber, @event.Amount, @event.MerchantName);

        // Alert when approaching daily limit
        if (@event.DailySpentTotal >= @event.DailySpentTotal / @event.Amount * 0.8m) // Approximating daily limit
            notification.SendDailyLimitWarning(@event.CardNumber, @event.DailySpentTotal);
    }
}