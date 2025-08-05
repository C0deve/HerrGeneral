using System.Collections.Concurrent;

namespace HerrGeneral.Core;

/// <summary>
/// Base class for event dispatcher (write side and read side)
/// </summary>
internal abstract class EventDispatcherBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, IEventHandlerWrapper> _eventHandlerWrappers = new();

    /// <summary>
    /// Type of the wrapper used to dispatch events
    /// </summary>
    protected abstract Type WrapperOpenType { get; }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    protected EventDispatcherBase(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Dispatch the event using an instance of WrapperType
    /// </summary>
    /// <param name="unitOfWorkId"></param>
    /// <param name="eventToDispatch"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual void Dispatch(UnitOfWorkId unitOfWorkId, object eventToDispatch, CancellationToken cancellationToken)
    {
        var wrapper = _eventHandlerWrappers.GetOrAdd(eventToDispatch.GetType(), eventTypeInput =>
        {
            var wrapperType = WrapperOpenType.MakeGenericType(eventTypeInput);
            var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {eventToDispatch.GetType()}");
            return (IEventHandlerWrapper)wrapper;
        });

        wrapper.Handle(unitOfWorkId, eventToDispatch, _serviceProvider);
    }
}