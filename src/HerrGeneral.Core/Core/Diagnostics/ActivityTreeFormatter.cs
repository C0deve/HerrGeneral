using System.Globalization;
using System.Text;

namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Formats activity traces and command execution logs into a hierarchical ASCII causal tree.
/// </summary>
public static class ActivityTreeFormatter
{
    private const int HeaderTotalWidth = 80;

    /// <summary>
    /// Formats the collected activity trace into an ASCII tree string representation.
    /// </summary>
    internal static string Format(ActivityTreeCollector collector)
    {
        var sb = new StringBuilder(512);

        RenderHeader(sb, collector);
        RenderCommandHandler(sb, collector);
        RenderWriteSide(sb, collector);
        RenderReadSide(sb, collector);

        return sb.ToString();
    }

    private static void RenderHeader(StringBuilder sb, ActivityTreeCollector collector)
    {
        var cmdName = collector.CommandName ?? "UnknownCommand";
        var threadId = collector.ThreadId ?? Environment.CurrentManagedThreadId;
        var status = collector.IsSuccess ? "OK" : "FAILED";
        var totalDurationStr = FormatDuration(collector.TotalDuration);

        var left = $"CMD [{cmdName}] (thread #{threadId}) ";
        var right = $" [{status}] ({totalDurationStr})";
        RenderDottedHeader(sb, left, right);
    }

    private static void RenderCommandHandler(StringBuilder sb, ActivityTreeCollector collector)
    {
        sb.AppendLine(" |");

        var handlerName = collector.HandlerType != null
            ? collector.HandlerType.GetFriendlyName()
            : "Handler";
        var totalDurationStr = FormatDuration(collector.TotalDuration);

        if (collector is { IsSuccess: false, CommandException: not null })
        {
            RenderDottedHeader(sb, $" \\--> (cmd) {handlerName} ", " [ERR]");
            sb.AppendLine("       |");
            RenderExceptionBlock(sb, "       ", collector.CommandException, collector.TotalDuration);
        }
        else
        {
            sb.AppendLine($" \\--> (cmd) {handlerName} ({totalDurationStr})");
        }
    }

    private static void RenderWriteSide(StringBuilder sb, ActivityTreeCollector collector)
    {
        var hasUow = collector.UnitOfWork is { Started: true };
        var hasWriteSideEvents = collector.RootWriteSideEvents.Count > 0;

        if (hasUow)
        {
            sb.AppendLine("       |");

            if (hasWriteSideEvents)
            {
                RenderWriteSideEvents(sb, collector.RootWriteSideEvents, "       ", isUowRoot: true);
                sb.AppendLine("       |");
            }

            if (collector.UnitOfWork!.RolledBack)
            {
                var rollbackDur = collector.UnitOfWork.RollbackDuration;
                sb.AppendLine($"       \\== [TX ROLLBACK] ({FormatDuration(rollbackDur)})");
            }
            else
            {
                var commitDur = collector.UnitOfWork.CommitDuration;
                sb.AppendLine($"       \\== [TX COMMIT] ({FormatDuration(commitDur)})");
            }
        }
        else if (hasWriteSideEvents)
        {
            sb.AppendLine("       |");
            RenderWriteSideEvents(sb, collector.RootWriteSideEvents, "       ");
        }
    }

    private static void RenderReadSide(StringBuilder sb, ActivityTreeCollector collector)
    {
        if (collector.UnitOfWork is { RolledBack: true })
        {
            sb.AppendLine("       |");
            sb.AppendLine("       +--x [SYNC PROJECTIONS] (Skipped: Transaction aborted)");
            sb.AppendLine("       \\--x [POST TRANSACTION] (Skipped: Transaction aborted)");
            return;
        }

        var hasSync = collector.SyncProjections.Count > 0;
        var hasPost = collector.PostTransactions.Count > 0;

        if (hasSync)
        {
            var branch = hasPost ? "+--" : "\\--";
            var itemIndent = hasPost ? "       |    " : "            ";
            RenderProjectionSection(sb, "[SYNC PROJECTIONS] (Read-Side)", branch, itemIndent, collector.SyncProjections);
        }

        if (hasPost)
        {
            RenderProjectionSection(sb, "[POST TRANSACTION] (Side Effects & Outbox)", "\\--", "            ", collector.PostTransactions);
        }
        else if (!hasSync && collector.ReadSideHandlers.Count > 0)
        {
            RenderProjectionSection(sb, "[READ SIDE] (Projections)", "\\--", "            ", collector.ReadSideHandlers);
        }
    }

    private static void RenderProjectionSection(
        StringBuilder sb,
        string title,
        string branch,
        string itemIndent,
        IReadOnlyList<IProjectionTraceNode> items)
    {
        sb.AppendLine("       |");
        sb.AppendLine($"       {branch} {title}");

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var itemBranch = (i == items.Count - 1) ? "\\--" : "|--";
            RenderProjectionLine(sb, itemIndent, itemBranch, item.EventType, item.HandlerType, item.Duration, item.Exception);
        }
    }

    private static void RenderProjectionLine(
        StringBuilder sb,
        string indent,
        string branch,
        Type eventType,
        Type handlerType,
        TimeSpan duration,
        ExceptionInfo? exception)
    {
        var evtName = eventType.GetFriendlyName();
        var hName = handlerType.GetFriendlyName();
        var status = exception != null ? "[ERR]" : $"({FormatDuration(duration)})";
        sb.AppendLine($"{indent}{branch} {evtName,-28} ===> {hName,-24} {status}");
    }

    private static void RenderWriteSideEvents(StringBuilder sb, IReadOnlyList<WriteSideEventNode> events, string currentIndent, bool isUowRoot = false)
    {
        for (var i = 0; i < events.Count; i++)
        {
            var evt = events[i];
            var isLastEvent = i == events.Count - 1;
            RenderWriteSideEvent(sb, evt, currentIndent, isLastEvent, isUowRoot);
        }
    }

    private static void RenderWriteSideEvent(StringBuilder sb, WriteSideEventNode evt, string currentIndent, bool isLastEvent, bool isUowRoot)
    {
        var eventBranch = isUowRoot ? "|--" : (isLastEvent ? "\\--" : "|--");
        sb.AppendLine($"{currentIndent}{eventBranch} (evt) {evt.EventType.GetFriendlyName()}");

        if (evt.Handlers.Count > 0)
        {
            var connectorIndent = currentIndent + (isUowRoot || !isLastEvent ? "|    " : "     ");
            RenderWriteSideHandlers(sb, evt.Handlers, connectorIndent);

            if (!isLastEvent)
            {
                sb.AppendLine($"{currentIndent}|");
            }
        }
    }

    private static void RenderWriteSideHandlers(StringBuilder sb, IReadOnlyList<WriteSideHandlerNode> handlers, string connectorIndent)
    {
        for (var j = 0; j < handlers.Count; j++)
        {
            var h = handlers[j];
            var isLastHandler = j == handlers.Count - 1;
            RenderWriteSideHandler(sb, h, connectorIndent, isLastHandler);
        }
    }

    private static void RenderWriteSideHandler(StringBuilder sb, WriteSideHandlerNode h, string connectorIndent, bool isLastHandler)
    {
        var handlerBranch = isLastHandler ? "\\-->" : "+-->";
        var prefix = $"{connectorIndent}{handlerBranch} (wr) {h.HandlerType.GetFriendlyName()}";

        if (h.Exception != null)
        {
            RenderDottedHeader(sb, $"{prefix} ", " [ERR]");
            sb.AppendLine($"{connectorIndent}      |");
            RenderExceptionBlock(sb, $"{connectorIndent}      ", h.Exception, h.Duration);
        }
        else
        {
            sb.AppendLine($"{prefix} ({FormatDuration(h.Duration)})");

            if (h.ChildEvents.Count > 0)
            {
                RenderWriteSideEvents(sb, h.ChildEvents, connectorIndent + "      ", isUowRoot: false);
            }
        }

        if (!isLastHandler && (h.ChildEvents.Count > 0 || h.Exception != null))
        {
            sb.AppendLine($"{connectorIndent}|");
        }
    }

    private static void RenderExceptionBlock(StringBuilder sb, string indent, ExceptionInfo exception, TimeSpan duration)
    {
        sb.AppendLine($"{indent}\\--! EXCEPTION: {exception.Type.GetFriendlyName()} ({FormatDuration(duration)})");
        sb.AppendLine($"{indent}     Message : \"{exception.Message}\"");
        if (!string.IsNullOrWhiteSpace(exception.Origin))
        {
            sb.AppendLine($"{indent}     Origin  : {exception.Origin}");
        }
    }

    private static void RenderDottedHeader(StringBuilder sb, string prefix, string suffix)
    {
        var dotsCount = Math.Max(3, HeaderTotalWidth - prefix.Length - suffix.Length);
        sb.Append(prefix).Append('.', dotsCount).AppendLine(suffix);
    }

    private static string FormatDuration(TimeSpan duration) => duration.TotalMilliseconds switch
    {
        < 0.05 and > 0 => "< 0.1ms",
        < 1000 => string.Create(CultureInfo.InvariantCulture, $"{duration.TotalMilliseconds:0.0}ms"),
        _ => string.Create(CultureInfo.InvariantCulture, $"{duration.TotalSeconds:0.00}s")
    };
}
