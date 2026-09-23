namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Formats activity traces and command execution logs into a hierarchical ASCII tree.
/// </summary>
public static class ActivityTreeFormatter
{
    /// <summary>
    /// Formats the collected activity trace into an ASCII tree string representation.
    /// </summary>
    internal static string Format(ActivityTreeCollector collector)
    {
        return collector.BuildString();
    }
}
