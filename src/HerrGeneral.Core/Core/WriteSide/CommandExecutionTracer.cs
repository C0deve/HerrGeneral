using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Legacy CommandExecutionTracer, now delegating to ActivityTreeCollector.
/// </summary>
[Obsolete("Replaced by ActivityTreeCollector and OpenTelemetry ActivitySource.")]
internal class CommandExecutionTracer : ActivityTreeCollector
{
}
