
namespace HerrGeneral.ReadSide;

/// <summary>
/// Defines a handler that processes events to update read-side projections
/// </summary>
/// <typeparam name="TEvent">The type of event being handled</typeparam>
public interface IProjectionEventHandler<in TEvent> : IHandlePostProjection<TEvent>
{
}