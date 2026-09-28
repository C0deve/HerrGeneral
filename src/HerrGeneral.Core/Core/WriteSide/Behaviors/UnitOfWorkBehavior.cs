using HerrGeneral.Core.Diagnostics;
using HerrGeneral.Core.WriteSide.Tracer;
using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Core.WriteSide.Behaviors;

internal sealed class UnitOfWorkBehavior<TCommand, TResult>(IUnitOfWork? unitOfWork, ActivityTreeCollector? collector = null)
    : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
{
    public PipelinePhase Phase => PipelinePhase.Transaction;
    public int OrderWithinPhase => 0;

    public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> next)
    {
        if (unitOfWork is null)
        {
            return await next().ConfigureAwait(false);
        }

        var uow = unitOfWork is UnitOfWorkTraceDecorator ? unitOfWork : new UnitOfWorkTraceDecorator(unitOfWork, collector);

        using (uow)
        {
            try
            {
                uow.Start();
                var result = await next().ConfigureAwait(false);
                uow.Commit();
                return result;
            }
            catch (System.Exception)
            {
                uow.RollBack();
                throw;
            }
        }
    }
}
