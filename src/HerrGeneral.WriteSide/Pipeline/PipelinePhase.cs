namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Defines the canonical execution phases of the command processing pipeline.
/// Lower phase values execute first (outer wrapper), and higher values execute closer to the core handler (inner wrapper).
/// </summary>
public enum PipelinePhase
{
    /// <summary>
    /// Diagnostics, logging, tracing and metric collection wrapping the entire execution.
    /// </summary>
    Diagnostics = 100,

    /// <summary>
    /// Security checks, authentication and authorization verification before execution.
    /// </summary>
    Security = 200,

    /// <summary>
    /// Resilience mechanisms such as concurrency limiting, retry policies and circuit breakers.
    /// </summary>
    Resilience = 300,

    /// <summary>
    /// Command validation and precondition checks before transactional boundary.
    /// </summary>
    Validation = 400,

    /// <summary>
    /// Transactional boundary management (e.g. Unit of Work start, commit and rollback).
    /// </summary>
    Transaction = 500,

    /// <summary>
    /// Core handler execution and synchronous in-transaction projections / cascading events.
    /// </summary>
    Core = 600
}
