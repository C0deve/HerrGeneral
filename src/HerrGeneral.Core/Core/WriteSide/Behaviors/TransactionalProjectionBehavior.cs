using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Core.WriteSide.Behaviors;

internal sealed class TransactionalProjectionBehavior<TCommand, TResult>(TransactionalProjectionEventDispatcher dispatcher)
    : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
{
    public PipelinePhase Phase => PipelinePhase.Core;
    public int OrderWithinPhase => -30;

    public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> next)
    {
        var result = await next().ConfigureAwait(false);
        dispatcher.Dispatch(result.Events);
        return result;
    }
}
