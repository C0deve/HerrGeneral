using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Map <see cref="IEventHandler{TEvent}"/> to <see cref="HerrGeneral.WriteSide.IEventHandler{TEvent}"/>
/// </summary>
internal class EventHandlerInternal<TEvent, THandler>(THandler handler) : HerrGeneral.WriteSide.IEventHandler<TEvent>
    where THandler : IEventHandler<TEvent>
{
    private readonly THandler _handler = handler;

    public void Handle(TEvent notification) => _handler.Handle(notification);
}