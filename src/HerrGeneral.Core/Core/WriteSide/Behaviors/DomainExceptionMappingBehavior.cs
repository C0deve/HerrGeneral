using System.Runtime.ExceptionServices;
using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Core.WriteSide.Behaviors;

internal sealed class DomainExceptionMappingBehavior<TCommand, TResult>(DomainExceptionMapper mapper)
    : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
{
    public PipelinePhase Phase => PipelinePhase.Core;
    public int OrderWithinPhase => -10;

    public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> next)
    {
        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (System.Exception e)
        {
            var mapped = mapper.Map(e,
                exception => new DomainException(exception),
                exception => exception);

            if (ReferenceEquals(mapped, e))
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }

            throw mapped;
        }
    }
}
