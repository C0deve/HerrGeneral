namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class EventTracker
{
    private readonly List<EventBase> _events = [];
    public IEnumerable<EventBase> GetEventsWithSourceCommandId(Guid sourceCommandId) => _events.Where(x => x.SourceCommandId ==  sourceCommandId);
    public void AddHandled(EventBase @event) => _events.Add(@event);
}