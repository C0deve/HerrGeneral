namespace HerrGeneral.OpenTelemetry;

using System;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;
using Core.Diagnostics;

/// <summary>
/// Extension methods for configuring HerrGeneral OpenTelemetry instrumentation.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds HerrGeneral tracing instrumentation to the <see cref="TracerProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static TracerProviderBuilder AddHerrGeneralInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource(HerrGeneralDiagnostics.ActivitySourceName);
    }

    /// <summary>
    /// Adds HerrGeneral metrics instrumentation to the <see cref="MeterProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static MeterProviderBuilder AddHerrGeneralInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMeter(HerrGeneralDiagnostics.MeterName);
    }
}
