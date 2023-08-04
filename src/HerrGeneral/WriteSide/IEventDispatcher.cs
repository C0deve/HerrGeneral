using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.WriteSide;

public interface IEventDispatcher
{
    Task Dispatch(IEvent eventToDispatch, CancellationToken cancellationToken);
}