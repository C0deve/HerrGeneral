using Divergic.Logging.Xunit;

namespace HerrGeneral.Test.Extension.Log;

public class LogConfig : LoggingConfig
{
    private LogConfig()
    {
        Formatter = new SimpleLogFormatter();
    }

    public static LogConfig Current { get; } = new();
}