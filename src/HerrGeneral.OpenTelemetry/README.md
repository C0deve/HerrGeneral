# HerrGeneral.OpenTelemetry

Extension package providing seamless OpenTelemetry instrumentation for **HerrGeneral** CQRS library.

## Installation

```bash
dotnet add package HerrGeneral.OpenTelemetry
```

## Quick Start

Register HerrGeneral distributed tracing and metrics with the OpenTelemetry .NET SDK:

```csharp
using HerrGeneral.OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddHerrGeneralInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddHerrGeneralInstrumentation()
        .AddOtlpExporter());
```
