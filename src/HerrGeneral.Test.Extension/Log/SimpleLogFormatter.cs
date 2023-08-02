using System.Text;
using Divergic.Logging.Xunit;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Test.Extension.Log;

public class SimpleLogFormatter : ILogFormatter
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
        
        if (!string.IsNullOrEmpty(message)) builder.Append(message);

        if (exception != null) builder.Append($"Exception {exception}");

        return builder.ToString();
    }
}