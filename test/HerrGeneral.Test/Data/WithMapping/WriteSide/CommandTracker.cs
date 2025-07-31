namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public abstract class CommandTracker
{
    private readonly HashSet<Guid> _handledCommandIds = [];
    public bool HasHandled(Guid commandId) => _handledCommandIds.Contains(commandId);
    public void AddHandled(Guid commandId) => _handledCommandIds.Add(commandId);
}

public class CommandTracker1 : CommandTracker;
public class CommandTracker2 : CommandTracker;
public class CommandTracker3 : CommandTracker;
