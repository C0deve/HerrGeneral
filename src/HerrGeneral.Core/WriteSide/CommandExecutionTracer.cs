using System.Text;
using HerrGeneral.Core.Error;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// CommandExecutionTracer logs and traces the execution flow of commands and events in the system.
/// Responsible for tracking command handling, event publishing, and capturing exceptions during processing.
/// This provides a complete audit trail of command and event processing, making debugging and monitoring easier.
/// </summary>
internal class CommandExecutionTracer
{
    private const string Indent = "      ";
    private readonly StringBuilder _stringBuilder = new();

    private static string BuildIndent(int indentCount) =>
        Enumerable.Range(0, indentCount)
            .Select(_ => Indent)
            .Aggregate(string.Empty, (s, s1) => s + s1);

    public void StartHandlingCommand(string type) =>
        _stringBuilder.AppendLine($"<------------------- {type} thread<{Environment.CurrentManagedThreadId}> ------------------->");

    public void StopHandlingCommand(string type, TimeSpan elapsed) =>
        _stringBuilder.AppendLine($"<------------------- {type} Finished {elapsed:c} -------------------/>");

    public void PublishEventOnWriteSide(object @event) =>
        _stringBuilder
            .AppendLine($"|| Publish Write Side on thread<{Environment.CurrentManagedThreadId}>")
            .AppendLine($"{Indent}{@event.GetType()}");

    public void HandleEvent(Type tHandler) =>
        _stringBuilder.AppendLine($"{Indent}-> Handle by {tHandler}");

    public void OnException(DomainException e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        _stringBuilder.AppendLine($"{indent}!! {e.InnerException?.GetType()} (DomainException)")
            .AppendLine($"{indent}-- Message : {e.InnerException?.Message}");
    }

    public void OnException(Exception e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        _stringBuilder.AppendLine($"{indent}!! {e.InnerException?.GetType() ?? e.GetType()} (PanicException)")
            .AppendLine($"{indent}-- Message : {e.Message}")
            .AppendLine($"{indent}-- StackTrace :{e.StackTrace}");
    }

    public void StartPublishEventsOnReadSide(int eventsToPublishCount)
    {
        if (eventsToPublishCount <= 0) return;

        _stringBuilder
            .AppendLine()
            .AppendLine($"|| Publish Read Side ({eventsToPublishCount} event{(eventsToPublishCount > 1 ? "s" : string.Empty)}) on thread<{Environment.CurrentManagedThreadId}>");
    }

    public void PublishEventOnReadSide(object @event) =>
        _stringBuilder.AppendLine($"{Indent}{@event.GetType()}");

    public string BuildString() => _stringBuilder.ToString();
}