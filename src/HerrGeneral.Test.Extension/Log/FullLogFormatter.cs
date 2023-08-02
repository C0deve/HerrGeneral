using System.Text;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Test.Extension.Log;

public class FullLogFormatter : ILogFormatter
{
    public string Format(
        int scopeLevel,
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        string message,
        Exception? exception)
    {
        var builder = new StringBuilder();

        if (scopeLevel > 0) builder.Append(' ', scopeLevel * 2);

        builder.AppendLine($"{logLevel} ");
        
        if (!string.IsNullOrEmpty(categoryName)) builder.Append($"{categoryName} ");

        if (eventId.Id != 0) builder.Append($"[{eventId.Id}]: ");

        if (!string.IsNullOrEmpty(message)) builder.Append(message);

        if (exception != null) builder.AppendLine($"{exception}");

        return builder.ToString();
    }
}