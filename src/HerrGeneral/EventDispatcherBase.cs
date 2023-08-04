using System.Collections.Concurrent;
using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral;

public abstract class EventDispatcherBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _eventHandlerWrappers = new();
    protected abstract Type WrapperOpenType { get; }

    protected EventDispatcherBase(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public virtual async Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken)
    {

        var wrapper = _eventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), eventTypeInput =>
        {
            var wrapperType = WrapperOpenType.MakeGenericType(eventTypeInput);
            return (IEventHandlerWrapper)(Activator.CreateInstance(wrapperType) ?? throw new DispatcherException($"Could not create wrapper type for {eventToDispatch.GetType()}"));
        });

        await wrapper.Handle(eventToDispatch, _serviceProvider, cancellationToken);
    }
}