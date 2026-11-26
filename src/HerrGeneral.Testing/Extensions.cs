using HerrGeneral.Testing.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Testing;

/// <summary>
/// Extension methods for testing code using HerrGeneral.Core
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Register a logger that writes to the test output, whatever the test framework and its version.
    /// With xUnit: <c>services.AddHerrGeneralTestLogger(testOutputHelper.WriteLine)</c>.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="writeLine">Writes one line to the test output</param>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public static IServiceCollection AddHerrGeneralTestLogger(this IServiceCollection serviceCollection, Action<string> writeLine, LogLevel logLevel = LogLevel.Debug) =>
        serviceCollection.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(logLevel)
                .AddProvider(new TestOutputLoggerProvider(writeLine));
        });
}
