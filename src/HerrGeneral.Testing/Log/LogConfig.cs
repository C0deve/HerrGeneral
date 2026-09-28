using Divergic.Logging.Xunit;

namespace HerrGeneral.Testing.Log;

/// <summary>
/// Logger configuration
/// </summary>
public sealed class LogConfig : LoggingConfig
{
    private LogConfig() => Formatter = new MessageOnlyLogFormatter();

    /// <summary>
    /// Current logger configuration
    /// </summary>
    public static LogConfig Current { get; } = new();
}