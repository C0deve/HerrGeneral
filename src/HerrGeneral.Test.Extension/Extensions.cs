using HerrGeneral.Test.Extension.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HerrGeneral.Test.Extension;

/// <summary>
/// Extension methods for testing code using HerrGeneral.Core
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Register a logger to writes to the xUnit test output.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="testOutputHelper"></param>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public static IServiceCollection AddHerrGeneralTestLogger(this IServiceCollection serviceCollection, ITestOutputHelper testOutputHelper, LogLevel logLevel = LogLevel.Debug) =>
        serviceCollection.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(logLevel)
                .AddXunit(testOutputHelper, LogConfig.Current);
        });
}