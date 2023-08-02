using System.Text;
using HerrGeneral.Contracts;
using HerrGeneral.Contracts.WriteSide;
using HerrGeneral.Error;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Logger;

internal static class Extensions
{
    private const string Indent = "      ";

    public static string EvaluateType(this Type type)
    {
        if (!type.IsGenericType) return type.ToString();

        var retType = new StringBuilder();

        var parentType = type.FullName?.Split('`');
        // We will build the type here.
        var arguments = type.GetGenericArguments();

        var argList = new StringBuilder();
        foreach (var t in arguments)
        {
            // Let's make sure we get the argument list.
            var arg = EvaluateType(t);
            if (argList.Length > 0)
            {
                argList.Append($", {arg}");
            }
            else
            {
                argList.Append(arg);
            }
        }

        if (argList.Length <= 0)
            return retType.ToString();

        if (parentType != null)
            retType.Append($"{parentType[0]}<{argList}>");

        return retType.ToString();
    }

    public static void StartHandling<TCommand, TResult>(this ILogger<ICommandHandler<TCommand, TResult>> logger, string type, TCommand command) where TCommand : ICommand<TResult> =>
        logger.LogInformation("<------------------- Handle {Type}{CommandId} thread<{CurrentManagedThreadId}> ------------------->\n{Command}",
            type,
            $" <{command.Id}>",
            Environment.CurrentManagedThreadId,
            command.Log(new StringBuilder()).ToString()
        );

    public static void StopHandling(this ILogger logger, string type, TimeSpan elapsed) =>
        logger.LogInformation("<------------------- {Type} Finished {ElapsedTime:c} -------------------/>",
            type,
            elapsed);

    public static void StartHandling<TEvent>(this ILogger<IEventHandler<TEvent>> logger, IEventHandler<TEvent> handler, TEvent @event) where TEvent : IEvent =>
        logger.LogInformation("{Indent}<{HandlerType} {EventId} thread'{CurrentManagedThreadId}'>",
            Indent,
            handler.GetType().GetFriendlyName(),
            $" <{@event.EventId}>",
            Environment.CurrentManagedThreadId
        );

    public static void StopHandling<TEvent>(this ILogger logger, IEventHandler<TEvent> handler) where TEvent : IEvent =>
        logger.LogInformation("{Indent}<{HandlerType}/>",
            Indent,
            handler.GetType().GetFriendlyName());
    
    public static void Log(this ILogger logger, DomainException e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        logger.LogError("{Indent}!! DomainException of type {FriendlyName}\n{Indent2}-- Message : {DomainErrorMessage}\n{Indent3}-- StackTrace :{StackTrace}\n",
            indent,
            e.DomainError.GetType().GetFriendlyName(),
            indent,
            e.DomainError.Message,
            indent,
            e.StackTrace);
    }

    private static string BuildIndent(int indentCount) => Enumerable.Range(0, indentCount).Select(_ => Indent).Aggregate(string.Empty, (s, s1) => s + s1);

    public static void Log(this ILogger logger, Exception e, int indentCount = 0)
    {
        var indent = BuildIndent(indentCount);
        logger.LogError("{Indent}!! Panic exception of type {FriendlyName}\n{Indent2}-- Message : {Message}\n{Indent3}-- StackTrace :{StackTrace}\n",
            indent,
            e.GetType().GetFriendlyName(),
            indent,
            e.Message, 
            indent,
            e.StackTrace);
    }

    public static void LogReadSideEventHandler(this ILogger logger, Type tHandler) =>
        logger.LogInformation("{Indent}** Handle by {Type}", Indent, tHandler);

    public static void LogReadSidePublishStart(this ILogger logger, int eventsToPublishCount) =>
        logger.LogInformation("-- || Publish Read Side, {Count} event(s), thread<{CurrentManagedThreadId}>",
            eventsToPublishCount,
            Environment.CurrentManagedThreadId);

    public static void LogPublishEventOnReadSide(this ILogger logger, IEvent @event)
    {
        logger.LogInformation("{Indent}{EventType}", Indent, @event.GetType());
        logger.Log(@event);
    }

    private static void Log(this ILogger logger, IEvent @event)
    {
        const string indent = Indent + Indent;
        logger.LogInformation("{Event}", @event.Log(new StringBuilder(), indent).ToString());
    }
}