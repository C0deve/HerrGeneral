namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Collects execution trace events during command handling for hierarchical ASCII causal tree formatting.
/// </summary>
internal class ActivityTreeCollector
{
    private readonly Lock _lock = new();

    public string? CommandName { get; private set; }
    public Type? HandlerType { get; private set; }
    public int? ThreadId { get; private set; }
    public TimeSpan TotalDuration { get; private set; }
    public bool IsSuccess { get; private set; } = true;
    public ExceptionInfo? CommandException { get; private set; }

    public UnitOfWorkTrace? UnitOfWork { get; private set; }

    public List<WriteSideEventNode> RootWriteSideEvents { get; } = new();
    private readonly Dictionary<object, WriteSideEventNode> _eventNodes = new(ReferenceEqualityComparer.Instance);
    private object? _currentWriteSideEvent;

    public List<SyncProjectionTraceNode> SyncProjections { get; } = new();
    public List<PostTransactionTraceNode> PostTransactions { get; } = new();
    public List<ReadSideTraceNode> ReadSideHandlers { get; } = new();

    public void StartHandlingCommand(string commandName, Type handlerType, int? threadId = null)
    {
        lock (_lock)
        {
            CommandName = commandName;
            HandlerType = handlerType;
            ThreadId = threadId ?? Environment.CurrentManagedThreadId;
        }
    }

    public void StopHandlingCommand(string commandName, TimeSpan elapsed)
    {
        lock (_lock)
        {
            CommandName ??= commandName;
            TotalDuration = elapsed;
        }
    }

    public void RecordCommandException(System.Exception exception)
    {
        lock (_lock)
        {
            IsSuccess = false;
            CommandException = ExceptionInfo.From(exception);
        }
    }

    public void StartUnitOfWork(TimeSpan? duration = null)
    {
        lock (_lock)
        {
            UnitOfWork ??= new UnitOfWorkTrace();
            UnitOfWork.Started = true;
            if (duration.HasValue) UnitOfWork.StartDuration = duration.Value;
        }
    }

    public void CommitUnitOfWork(TimeSpan? duration = null)
    {
        lock (_lock)
        {
            UnitOfWork ??= new UnitOfWorkTrace();
            UnitOfWork.Committed = true;
            if (duration.HasValue) UnitOfWork.CommitDuration = duration.Value;
        }
    }

    public void RollbackUnitOfWork(TimeSpan? duration = null)
    {
        lock (_lock)
        {
            IsSuccess = false;
            UnitOfWork ??= new UnitOfWorkTrace();
            UnitOfWork.RolledBack = true;
            if (duration.HasValue) UnitOfWork.RollbackDuration = duration.Value;
        }
    }

    public void DisposeUnitOfWork()
    {
        lock (_lock)
        {
            if (UnitOfWork != null) UnitOfWork.Disposed = true;
        }
    }

    public void RegisterRootWriteSideEvent(object @event)
    {
        lock (_lock)
        {
            if (_eventNodes.TryGetValue(@event, out var node)) return;
            node = new WriteSideEventNode(@event.GetType());
            _eventNodes[@event] = node;
            RootWriteSideEvents.Add(node);
        }
    }

    public void PublishEventOnWriteSide(object @event)
    {
        lock (_lock)
        {
            _currentWriteSideEvent = @event;
            if (_eventNodes.TryGetValue(@event, out var node)) return;
            node = new WriteSideEventNode(@event.GetType());
            _eventNodes[@event] = node;
            RootWriteSideEvents.Add(node);
        }
    }

    public void RecordWriteSideHandler(Type handlerType, Type eventType, TimeSpan duration, IReadOnlyList<object>? childEvents, System.Exception? exception)
    {
        lock (_lock)
        {
            WriteSideEventNode? eventNode = null;
            if (_currentWriteSideEvent != null)
            {
                _eventNodes.TryGetValue(_currentWriteSideEvent, out eventNode);
            }

            if (eventNode == null)
            {
                eventNode = RootWriteSideEvents.LastOrDefault(e => e.EventType == eventType);
                if (eventNode == null)
                {
                    eventNode = new WriteSideEventNode(eventType);
                    RootWriteSideEvents.Add(eventNode);
                }
            }

            var handlerNode = new WriteSideHandlerNode(handlerType, duration, exception != null ? ExceptionInfo.From(exception) : null);
            eventNode.Handlers.Add(handlerNode);

            if (exception != null)
            {
                IsSuccess = false;
            }

            if (childEvents is not { Count: > 0 }) return;
            foreach (var childEvent in childEvents)
            {
                var childNode = new WriteSideEventNode(childEvent.GetType());
                _eventNodes[childEvent] = childNode;
                handlerNode.ChildEvents.Add(childNode);
            }
        }
    }

    public void RecordSyncProjection(Type eventType, Type handlerType, TimeSpan duration, System.Exception? exception)
    {
        lock (_lock)
        {
            SyncProjections.Add(new SyncProjectionTraceNode(eventType, handlerType, duration, exception != null ? ExceptionInfo.From(exception) : null));
        }
    }

    public void RecordPostTransaction(Type eventType, Type handlerType, string kind, TimeSpan duration, System.Exception? exception)
    {
        lock (_lock)
        {
            PostTransactions.Add(new PostTransactionTraceNode(eventType, handlerType, kind, duration, exception != null ? ExceptionInfo.From(exception) : null));
        }
    }

    public void RecordReadSideHandler(Type eventType, Type handlerType, TimeSpan duration, System.Exception? exception)
    {
        lock (_lock)
        {
            ReadSideHandlers.Add(new ReadSideTraceNode(eventType, handlerType, duration, exception != null ? ExceptionInfo.From(exception) : null));
        }
    }

    public string BuildString()
    {
        return ActivityTreeFormatter.Format(this);
    }
}

internal class UnitOfWorkTrace
{
    public bool Started { get; set; }
    public TimeSpan StartDuration { get; set; }
    public bool Committed { get; set; }
    public TimeSpan CommitDuration { get; set; }
    public bool RolledBack { get; set; }
    public TimeSpan RollbackDuration { get; set; }
    public bool Disposed { get; set; }
}

internal class WriteSideEventNode(Type eventType)
{
    public Type EventType { get; } = eventType;
    public List<WriteSideHandlerNode> Handlers { get; } = new();
}

internal class WriteSideHandlerNode(Type handlerType, TimeSpan duration, ExceptionInfo? exception)
{
    public Type HandlerType { get; } = handlerType;
    public TimeSpan Duration { get; } = duration;
    public ExceptionInfo? Exception { get; } = exception;
    public List<WriteSideEventNode> ChildEvents { get; } = new();
}

internal interface IProjectionTraceNode
{
    Type EventType { get; }
    Type HandlerType { get; }
    TimeSpan Duration { get; }
    ExceptionInfo? Exception { get; }
}

internal record SyncProjectionTraceNode(Type EventType, Type HandlerType, TimeSpan Duration, ExceptionInfo? Exception) : IProjectionTraceNode;

internal record PostTransactionTraceNode(Type EventType, Type HandlerType, string Kind, TimeSpan Duration, ExceptionInfo? Exception) : IProjectionTraceNode;

internal record ReadSideTraceNode(Type EventType, Type HandlerType, TimeSpan Duration, ExceptionInfo? Exception) : IProjectionTraceNode;

internal partial record ExceptionInfo(Type Type, string Message, string? Origin)
{
    [System.Text.RegularExpressions.GeneratedRegex(@"<([^>]+)>")]
    private static partial System.Text.RegularExpressions.Regex GenericNameRegex();

    public static ExceptionInfo From(System.Exception exception)
    {
        var unwrapped = Unwrap(exception);
        return new ExceptionInfo(
            unwrapped.GetType(),
            unwrapped.Message,
            ExtractOrigin(unwrapped)
        );
    }

    private static System.Exception Unwrap(System.Exception exception)
    {
        var current = exception;
        while (current.InnerException is not null &&
               (current is DomainException ||
                current is EventHandlerDomainException ||
                current is EventHandlerException ||
                current is TargetInvocationException ||
                current is AggregateException))
        {
            if (current is AggregateException { InnerExceptions.Count: > 0 } agg)
            {
                current = agg.InnerExceptions[0];
            }
            else
            {
                current = current.InnerException;
            }
        }

        return current;
    }

    private static string? ExtractOrigin(System.Exception exception) =>
        ExtractFromStackTrace(exception)
        ?? ExtractFromRawStackTrace(exception.StackTrace)
        ?? ExtractFromTargetSite(exception);

    private static string? ExtractFromStackTrace(System.Exception exception)
    {
        try
        {
            var stackTrace = new System.Diagnostics.StackTrace(exception, true);
            var frames = stackTrace.GetFrames();
            if (frames.Length == 0) return null;

            return FindFirstValidFrame(frames, excludePipeline: true)
                ?? FindFirstValidFrame(frames, excludePipeline: false);
        }
        catch
        {
            return null;
        }
    }

    private static string? FindFirstValidFrame(System.Diagnostics.StackFrame[] frames, bool excludePipeline) => 
        frames
            .Select(frame => FormatFrame(frame, excludePipeline))
            .OfType<string>()
            .FirstOrDefault();

    private static string? ExtractFromRawStackTrace(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace)) return null;

        // First pass: find first line not in pipeline / system
        foreach (var rawLine in stackTrace.AsSpan().EnumerateLines())
        {
            var line = TrimStackTracePrefix(rawLine);
            if (!IsPipelineOrSystemNamespace(line))
            {
                return CleanStackTraceLine(line);
            }
        }

        // Second pass: find first non-system line
        foreach (var rawLine in stackTrace.AsSpan().EnumerateLines())
        {
            var line = TrimStackTracePrefix(rawLine);
            if (!IsSystemNamespace(line))
            {
                return CleanStackTraceLine(line);
            }
        }

        return null;
    }

    private static ReadOnlySpan<char> TrimStackTracePrefix(ReadOnlySpan<char> rawLine)
    {
        var line = rawLine.Trim();
        return line.StartsWith("at ") ? line[3..].Trim() : line;
    }

    private static string? ExtractFromTargetSite(System.Exception exception)
    {
        if (exception.TargetSite?.DeclaringType is null) return exception.Source;

        var declaringType = exception.TargetSite.DeclaringType;
        var fullTypeName = declaringType.FullName ?? declaringType.Name;

        return !IsPipelineOrSystemNamespace(fullTypeName)
            ? $"{fullTypeName}.{exception.TargetSite.Name}()"
            : exception.Source;
    }

    private static bool IsSystemNamespace(ReadOnlySpan<char> typeOrNamespace) =>
        typeOrNamespace.StartsWith("System.")
        || typeOrNamespace.StartsWith("Microsoft.");

    private static bool IsPipelineOrSystemNamespace(ReadOnlySpan<char> typeOrNamespace) =>
        typeOrNamespace.StartsWith("HerrGeneral.Core.")
        || typeOrNamespace.StartsWith("HerrGeneral.Core")
        || typeOrNamespace.StartsWith("HerrGeneral.Mediator")
        || IsSystemNamespace(typeOrNamespace);

    private static string? FormatFrame(System.Diagnostics.StackFrame frame, bool excludePipeline)
    {
        var method = frame.GetMethod();
        var declaringType = method?.DeclaringType;
        if (method is null || declaringType is null) return null;

        var fullTypeName = declaringType.FullName ?? declaringType.Name;
        if (ShouldIgnoreType(fullTypeName, excludePipeline)) return null;

        var (realType, methodName) = ResolveRealTypeAndMethod(method, declaringType);
        var realTypeName = (realType.FullName ?? realType.Name).Replace('+', '.');
        if (excludePipeline && IsPipelineOrSystemNamespace(realTypeName)) return null;

        return $"{realTypeName}.{methodName}()";
    }

    private static bool ShouldIgnoreType(string fullTypeName, bool excludePipeline) =>
        IsSystemNamespace(fullTypeName)
        || (excludePipeline && IsPipelineOrSystemNamespace(fullTypeName));

    private static (Type RealType, string MethodName) ResolveRealTypeAndMethod(MethodBase method, Type declaringType)
    {
        if (!IsCompilerGenerated(declaringType))
        {
            return (declaringType, method.Name);
        }

        var realType = declaringType.DeclaringType ?? declaringType;
        var methodName = ExtractMethodName(declaringType.Name)
            ?? ExtractMethodName(method.Name)
            ?? method.Name;

        return (realType, methodName);
    }

    private static bool IsCompilerGenerated(Type type) =>
        type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
        || type.Name.Contains('<');

    private static string? ExtractMethodName(string name)
    {
        var match = GenericNameRegex().Match(name);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string CleanStackTraceLine(ReadOnlySpan<char> line)
    {
        var inIndex = line.IndexOf(" in ");
        if (inIndex > 0)
        {
            line = line[..inIndex];
        }

        return line.Trim().ToString();
    }
}