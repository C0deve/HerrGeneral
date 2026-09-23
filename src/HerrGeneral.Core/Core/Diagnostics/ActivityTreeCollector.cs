using System.Text;

namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Collects execution trace events during command handling for hierarchical ASCII tree formatting.
/// </summary>
internal class ActivityTreeCollector
{
    private const string Indent = "      ";
    private readonly StringBuilder _stringBuilder = new();

    private static string BuildIndent(int indentCount) =>
        Enumerable.Range(0, indentCount)
            .Select(_ => Indent)
            .Aggregate(string.Empty, (s, s1) => s + s1);

    public void StartHandlingCommand(string commandName, Type handlerType, int? threadId = null)
    {
        var tid = threadId ?? Environment.CurrentManagedThreadId;
        lock (_stringBuilder)
        {
            _stringBuilder
                .AppendLine($"<------------------- {commandName} thread<{tid}> ------------------->")
                .AppendLine($"-> Handled by {handlerType}");
        }
    }

    public void StopHandlingCommand(string commandName, TimeSpan elapsed)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"<------------------- {commandName} Finished {elapsed:c} -------------------/>");
        }
    }

    public void StartPublishEventOnWriteSide(int? threadId = null)
    {
        var tid = threadId ?? Environment.CurrentManagedThreadId;
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"|| Publish Write Side on thread<{tid}>");
        }
    }

    public void StartUnitOfWork()
    {
        lock (_stringBuilder) { _stringBuilder.AppendLine("Start Unit of Work"); }
    }

    public void CommitUnitOfWork()
    {
        lock (_stringBuilder) { _stringBuilder.AppendLine("Commit Unit of Work"); }
    }

    public void RollbackUnitOfWork()
    {
        lock (_stringBuilder) { _stringBuilder.AppendLine("Rollback Unit of Work"); }
    }

    public void DisposeUnitOfWork()
    {
        lock (_stringBuilder) { _stringBuilder.AppendLine("Dispose Unit of Work"); }
    }

    public void PublishEventOnWriteSide(object @event)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");
        }
    }

    public void HandleEvent(Type tHandler)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}-> Handle by {tHandler}");
        }
    }

    public void OnException(DomainException e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{indent}!! {e.InnerException?.GetType()} (DomainException)")
                .AppendLine($"{indent}-- Message : {e.InnerException?.Message}");
        }
    }

    public void OnException(System.Exception e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{indent}!! {e.InnerException?.GetType() ?? e.GetType()} (PanicException)")
                .AppendLine($"{indent}-- Message : {e.Message}")
                .AppendLine($"{indent}-- StackTrace :{e.StackTrace}");
        }
    }

    public void StartPublishEventsOnReadSide(int eventsToPublishCount, int? threadId = null)
    {
        if (eventsToPublishCount <= 0) return;
        var tid = threadId ?? Environment.CurrentManagedThreadId;

        lock (_stringBuilder)
        {
            _stringBuilder
                .AppendLine()
                .AppendLine($"|| Publish Read Side ({eventsToPublishCount} event{(eventsToPublishCount > 1 ? "s" : string.Empty)}) on thread<{tid}>");
        }
    }

    public void PublishEventOnReadSide(object @event)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");
        }
    }

    public void StartPublishEventsOnSyncProjections(int eventsToPublishCount, int? threadId = null)
    {
        if (eventsToPublishCount <= 0) return;
        var tid = threadId ?? Environment.CurrentManagedThreadId;

        lock (_stringBuilder)
        {
            _stringBuilder
                .AppendLine()
                .AppendLine($"|| Publish Sync Projections ({eventsToPublishCount} event{(eventsToPublishCount > 1 ? "s" : string.Empty)}) on thread<{tid}>");
        }
    }

    public void PublishEventOnSyncProjections(object @event)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");
        }
    }

    public void HandleSyncProjection(Type handlerType)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}-> Handle sync projection by {handlerType}");
        }
    }

    public void StartPublishEventsOnPostTransaction(int eventsToPublishCount, int? threadId = null)
    {
        if (eventsToPublishCount <= 0) return;
        var tid = threadId ?? Environment.CurrentManagedThreadId;

        lock (_stringBuilder)
        {
            _stringBuilder
                .AppendLine()
                .AppendLine($"|| Publish Post Transaction ({eventsToPublishCount} event{(eventsToPublishCount > 1 ? "s" : string.Empty)}) on thread<{tid}>");
        }
    }

    public void PublishEventOnPostTransaction(object @event)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");
        }
    }

    public void StartPublishEventsOnSideEffects(int count, int? threadId = null)
    {
        if (count <= 0) return;
        var tid = threadId ?? Environment.CurrentManagedThreadId;

        lock (_stringBuilder)
        {
            _stringBuilder
                .AppendLine()
                .AppendLine($"|| Publish Side Effects ({count} event{(count > 1 ? "s" : string.Empty)}) on thread<{tid}>");
        }
    }

    public void PublishEventOnSideEffects(object @event)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");
        }
    }

    public void HandleSideEffectEvent(Type handlerType)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}-> Handle side-effect by {handlerType}");
        }
    }

    public void HandlePostProjectionEvent(Type handlerType)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{Indent}-> Handle post-projection by {handlerType}");
        }
    }

    public void OnSideEffectException(System.Exception e, int indentCount = 1)
    {
        var indent = BuildIndent(indentCount);
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{indent}!! Side-effect exception: {e.GetType().Name}")
                .AppendLine($"{indent}-- Message : {e.Message}");
        }
    }

    public void OnPostTransactionException(System.Exception e, Type handlerType, int indentCount = 1)
    {
        var indent = BuildIndent(indentCount);
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine($"{indent}!! Exception in post-transaction handler {handlerType}: {e.GetType().Name}")
                .AppendLine($"{indent}-- Message : {e.Message}");
        }
    }

    public string BuildString()
    {
        lock (_stringBuilder)
        {
            return _stringBuilder.ToString();
        }
    }

    public void AddTrace(string trace)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.AppendLine(trace);
        }
    }
}
