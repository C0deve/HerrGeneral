using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Central diagnostics configuration for HerrGeneral providing OpenTelemetry ActivitySource and Meter.
/// </summary>
public static class HerrGeneralDiagnostics
{
    /// <summary>
    /// ActivitySource name for HerrGeneral distributed tracing.
    /// </summary>
    public const string SourceName = "HerrGeneral";

    /// <summary>
    /// ActivitySource name for HerrGeneral distributed tracing (alias for <see cref="SourceName"/>).
    /// </summary>
    public const string ActivitySourceName = SourceName;

    /// <summary>
    /// Meter name for HerrGeneral metrics.
    /// </summary>
    public const string MeterName = "HerrGeneral";

    private static readonly string Version = typeof(HerrGeneralDiagnostics).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    /// <summary>
    /// The ActivitySource used for distributed tracing across HerrGeneral.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);

    /// <summary>
    /// The Meter used for publishing metrics across HerrGeneral.
    /// </summary>
    public static readonly Meter Meter = new(MeterName, Version);

    /// <summary>
    /// Counter for total commands executed.
    /// </summary>
    public static readonly Counter<long> CommandsTotal = Meter.CreateCounter<long>(
        "herrgeneral.commands.total",
        description: "Total number of executed commands");

    /// <summary>
    /// Histogram for command execution duration.
    /// </summary>
    public static readonly Histogram<double> CommandsDuration = Meter.CreateHistogram<double>(
        "herrgeneral.commands.duration",
        unit: "ms",
        description: "Duration of command execution in milliseconds");

    /// <summary>
    /// UpDownCounter for actively executing commands.
    /// </summary>
    public static readonly UpDownCounter<int> ActiveCommands = Meter.CreateUpDownCounter<int>(
        "herrgeneral.commands.active",
        description: "Number of commands currently executing");

    /// <summary>
    /// Counter for total events dispatched.
    /// </summary>
    public static readonly Counter<long> EventsTotal = Meter.CreateCounter<long>(
        "herrgeneral.events.total",
        description: "Total number of dispatched events");

    /// <summary>
    /// Histogram for event handler execution duration.
    /// </summary>
    public static readonly Histogram<double> EventsDuration = Meter.CreateHistogram<double>(
        "herrgeneral.events.duration",
        unit: "ms",
        description: "Duration of event handler execution in milliseconds");

    /// <summary>
    /// Starts a new Activity with the specified explicit name and kind.
    /// </summary>
    /// <remarks>
    /// Suppresses the 'ExplicitCallerInfoArgument' warning because <see cref="ActivitySource.StartActivity(string, ActivityKind, ActivityContext, IEnumerable{KeyValuePair{string, object?}}?, IEnumerable{ActivityLink}?, DateTimeOffset)"/>
    /// uses <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/> by default, whereas OpenTelemetry conventions require explicit semantic span names.
    /// Centralizing this call maximizes overall code clarity and eliminates repetitive inspection noise across the library.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "OpenTelemetry requires explicit semantic activity names rather than implicit caller method names.")]
    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        // Explicit caller info argument is intentional: OpenTelemetry requires predictable, semantic activity names instead of the caller method name.
        // ReSharper disable once ExplicitCallerInfoArgument
        return ActivitySource.StartActivity(name, kind);
    }

    /// <summary>
    /// Standard activity names for HerrGeneral tracing.
    /// </summary>
    public static class Activities
    {
        /// <summary>Activity name for command execution.</summary>
        public const string ExecuteCommand = "HerrGeneral.ExecuteCommand";
        /// <summary>Activity name for write-side event dispatching.</summary>
        public const string WriteSideDispatch = "HerrGeneral.WriteSide.Dispatch";
        /// <summary>Activity name for write-side event handling.</summary>
        public const string WriteSideHandleEvent = "HerrGeneral.WriteSide.HandleEvent";
        /// <summary>Activity name for unit of work.</summary>
        public const string UnitOfWork = "HerrGeneral.UnitOfWork";
        /// <summary>Activity name for synchronous projections dispatching.</summary>
        public const string SyncProjectionsDispatch = "HerrGeneral.SyncProjections.Dispatch";
        /// <summary>Activity name for synchronous projection event handling.</summary>
        public const string SyncProjectionsHandleEvent = "HerrGeneral.SyncProjections.HandleEvent";
        /// <summary>Activity name for post-transaction dispatching.</summary>
        public const string PostTransactionDispatch = "HerrGeneral.PostTransaction.Dispatch";
        /// <summary>Activity name for post-transaction event handling.</summary>
        public const string PostTransactionHandleEvent = "HerrGeneral.PostTransaction.HandleEvent";
        /// <summary>Activity name for read-side dispatching.</summary>
        public const string ReadSideDispatch = "HerrGeneral.ReadSide.Dispatch";
        /// <summary>Activity name for read-side event handling.</summary>
        public const string ReadSideHandleEvent = "HerrGeneral.ReadSide.HandleEvent";
    }

    /// <summary>
    /// Semantic attribute tags used in HerrGeneral traces.
    /// </summary>
    public static class Tags
    {
        /// <summary>Command name tag.</summary>
        public const string CommandName = "herrgeneral.command.name";
        /// <summary>Command type tag.</summary>
        public const string CommandType = "herrgeneral.command.type";
        /// <summary>Handler type tag.</summary>
        public const string HandlerType = "herrgeneral.handler.type";
        /// <summary>Event type tag.</summary>
        public const string EventType = "herrgeneral.event.type";
        /// <summary>Events count tag.</summary>
        public const string EventsCount = "herrgeneral.events.count";
        /// <summary>Execution status tag.</summary>
        public const string Status = "herrgeneral.status";
        /// <summary>Unit of work operation tag.</summary>
        public const string UowOperation = "herrgeneral.uow.operation";
        /// <summary>Managed thread ID tag.</summary>
        public const string ThreadId = "herrgeneral.thread.id";
        /// <summary>Post-transaction handler kind tag.</summary>
        public const string PostTransactionKind = "herrgeneral.post_transaction.kind";
        /// <summary>Exception type semantic tag.</summary>
        public const string ExceptionType = "exception.type";
        /// <summary>Exception message semantic tag.</summary>
        public const string ExceptionMessage = "exception.message";
        /// <summary>Exception stack trace semantic tag.</summary>
        public const string ExceptionStackTrace = "exception.stacktrace";
    }

    /// <summary>
    /// Records an exception as an ActivityEvent on the specified Activity following OpenTelemetry conventions.
    /// </summary>
    public static Activity? RecordException(this Activity? activity, System.Exception? exception)
    {
        if (activity is null || exception is null)
            return activity;

        var tags = new ActivityTagsCollection
        {
            { Tags.ExceptionType, exception.GetType().FullName },
            { Tags.ExceptionMessage, exception.Message },
            { Tags.ExceptionStackTrace, exception.ToString() }
        };

        activity.AddEvent(new ActivityEvent("exception", DateTimeOffset.UtcNow, tags));
        return activity;
    }
}
