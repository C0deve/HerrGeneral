namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Allows pipeline behaviors to specify their execution phase and ordering within that phase.
/// Behaviors with lower Phase and OrderWithinPhase execute first (outer wrapper).
/// </summary>
public interface IOrderedPipelineBehavior
{
    /// <summary>
    /// Gets the phase in which this behavior should execute. Defaults to <see cref="PipelinePhase.Core"/>.
    /// </summary>
    PipelinePhase Phase => PipelinePhase.Core;

    /// <summary>
    /// Gets the ordering priority within the phase. Lower numbers execute first. Defaults to 0.
    /// </summary>
    int OrderWithinPhase => 0;
}
