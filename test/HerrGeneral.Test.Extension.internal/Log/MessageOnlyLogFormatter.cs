using System.Text;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Test.Extension.Internal.Log;

/// <summary>
/// Display only the log message if provided
/// </summary>
public class MessageOnlyLogFormatter : ILogFormatter
{
    /// <summary>
    /// Format a log
    /// </summary>
    /// <param name="scopeLevel"></param>
    /// <param name="categoryName"></param>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public string Format(
        int scopeLevel,
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        string message,
        Exception? exception)
    {
        var builder = new StringBuilder();
        
        if (!string.IsNullOrEmpty(message)) builder.Append(message);

        if (exception != null) builder.Append($"Exception {exception}");

        return builder.ToString();
    }
}