using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Testing.Log;

/// <summary>
/// Writes each log, message only, to a test output (for xUnit: <c>testOutputHelper.WriteLine</c>).
/// Independent of the test framework and of its version.
/// </summary>
/// <param name="writeLine">Writes one line to the test output</param>
public sealed class TestOutputLoggerProvider(Action<string> writeLine) : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new TestOutputLogger(writeLine);

    /// <inheritdoc />
    public void Dispose()
    {
    }

    private sealed class TestOutputLogger(Action<string> writeLine) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception? exception, Func<TState, System.Exception?, string> formatter)
        {
            var builder = new StringBuilder(formatter(state, exception));
            if (exception != null) builder.Append(CultureInfo.InvariantCulture, $"Exception {exception}");
            if (builder.Length == 0) return;

            try
            {
                writeLine(builder.ToString());
            }
            catch (InvalidOperationException)
            {
                // xUnit refuses output once the test is over: a log written late by background work is dropped.
            }
        }
    }
}
