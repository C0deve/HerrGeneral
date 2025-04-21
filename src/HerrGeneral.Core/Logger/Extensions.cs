using System.Text;
using HerrGeneral.Contracts;
using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Logger;

internal static class Extensions
{
    private const string Indent = "      ";

    public static StringBuilder StartHandlingCommand<TCommand>(this StringBuilder logger, string type, TCommand command)
        where TCommand : CommandBase =>
        logger.AppendLine($"<------------------- {type} <{command.Id}> thread<{Environment.CurrentManagedThreadId}> ------------------->");

    public static void StopHandlingCommand(this StringBuilder logger, string type, TimeSpan elapsed) =>
        logger.AppendLine($"<------------------- {type} Finished {elapsed:c} -------------------/>");
    
    public static void PublishEventOnWriteSide(this StringBuilder logger, object @event) =>
        logger
            .AppendLine($"|| Publish Write Side on thread<{Environment.CurrentManagedThreadId}>")
            .AppendLine($"{Indent}{@event.GetType()}");
    
    public static void HandleEvent(this StringBuilder logger, Type tHandler) =>
        logger.AppendLine($"{Indent}-> Handle by {tHandler}");

    public static void OnException(this StringBuilder logger, DomainException e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        logger.AppendLine($"{indent}!! {e.DomainError.GetType()} (DomainException)")
            .AppendLine($"{indent}-- Message : {e.DomainError.Message}");
    }

    public static void OnException(this StringBuilder logger, Exception e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        logger.AppendLine($"{indent}!! {e.InnerException?.GetType() ?? e.GetType()} (PanicException)")
            .AppendLine($"{indent}-- Message : {e.Message}")
            .AppendLine($"{indent}-- StackTrace :{e.StackTrace}");
    }
    
    private static string BuildIndent(int indentCount) => 
        Enumerable.Range(0, indentCount)
            .Select(_ => Indent)
            .Aggregate(string.Empty, (s, s1) => s + s1);
    
    public static void StartPublishEventsOnReadSide(this StringBuilder logger, int eventsToPublishCount)
    {
        if (eventsToPublishCount <= 0) return;

        logger
            .AppendLine()
            .AppendLine($"|| Publish Read Side ({eventsToPublishCount} event{(eventsToPublishCount > 1 ? "s" : string.Empty)}) on thread<{Environment.CurrentManagedThreadId}>");
    }

    public static StringBuilder PublishEventOnReadSide(this StringBuilder logger, object @event) =>
        logger.AppendLine($"{Indent}{@event.GetType()}");

}