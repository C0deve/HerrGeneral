using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.DDD.Core;

/// <summary>
/// Internal adapter that bridges domain-specific DDD event handlers with the generic write-side interface.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
/// <typeparam name="THandler">The type of domain-specific event handler</typeparam>
/// <param name="handler">The domain-specific event handler that processes the event</param>
internal class VoidEventHandlerInternal<TEvent, THandler>(
    THandler handler) : WriteSide.IEventHandler<TEvent>, IHandlerTypeProvider
    where THandler : IVoidDomainEventHandler<TEvent>
{
    /// <summary>
    /// Handles the event by delegating to the domain handler
    /// </summary>
    /// <param name="notification">The event to handle</param>
    public IEnumerable<object> Handle(TEvent notification)
    {
        handler.Handle(notification);
        return [];
    }
    
    public Type GetHandlerType() => typeof(THandler);
}