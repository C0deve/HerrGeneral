using Divergic.Logging.Xunit;

namespace HerrGeneral.Test.Extension.Internal.Log;

/// <summary>
/// Logger configuration
/// </summary>
public class LogConfig : LoggingConfig
{
    private LogConfig() => Formatter = new MessageOnlyLogFormatter();

    /// <summary>
    /// Current logger configuration
    /// </summary>
    public static LogConfig Current { get; } = new();
}