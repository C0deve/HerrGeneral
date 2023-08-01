using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public interface IEventDispatcher
{
    Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken);
}