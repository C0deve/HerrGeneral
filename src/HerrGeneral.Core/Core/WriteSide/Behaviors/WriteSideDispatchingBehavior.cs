using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Core.WriteSide.Behaviors;

internal sealed class WriteSideDispatchingBehavior<TCommand, TResult>(WriteSideEventDispatcher dispatcher)
    : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
{
    public PipelinePhase Phase => PipelinePhase.Core;
    public int OrderWithinPhase => -20;

    public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> next)
    {
        var (events, result) = await next().ConfigureAwait(false);
        var dispatched = dispatcher.Dispatch(events);
        return (dispatched, result);
    }
}
