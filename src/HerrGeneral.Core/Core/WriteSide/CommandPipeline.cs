using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate (IReadOnlyList<object> Events, TResult Result) HandlerDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

    extension<TCommand, TResult>(HandlerDelegate<TCommand, TResult> next)
    {
        public HandlerDelegate<TCommand, TResult> WithDomainExceptionMapping(DomainExceptionMapper mapper) =>
            (command, cancellationToken) =>
            {
                try
                {
                    return next(command, cancellationToken);
                }
                catch (System.Exception e)
                {
                    var mapped = mapper.Map(e,
                        exception => new DomainException(exception),
                        exception => exception);

                    if (ReferenceEquals(mapped, e))
                    {
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e).Throw();
                    }

                    throw mapped;
                }
            };

        public HandlerDelegate<TCommand, TResult> WithUnitOfWork(IUnitOfWork? unitOfWork) =>
            (command, cancellationToken) =>
            {
                if (unitOfWork is null)
                {
                    return next(command, cancellationToken);
                }

                using (unitOfWork)
                {
                    try
                    {
                        unitOfWork.Start();
                        var result = next(command, cancellationToken);
                        unitOfWork.Commit();
                        return result;
                    }
                    catch (System.Exception)
                    {
                        unitOfWork.RollBack();
                        throw;
                    }
                }
            };

        public HandlerDelegate<TCommand, TResult> WithWriteSideDispatching(WriteSideEventDispatcher eventDispatcher) =>
            (command, cancellationToken) =>
            {
                var (events, result) = next(command, cancellationToken);
                return (eventDispatcher.Dispatch(events), result);
            };

        public HandlerDelegate<TCommand, TResult> WithTransactionalProjectionDispatching(TransactionalProjectionEventDispatcher transactionalProjectionEventDispatcher) =>
            (command, cancellationToken) =>
            {
                var result = next(command, cancellationToken);
                transactionalProjectionEventDispatcher.Dispatch(result.Events);
                return result;
            };

        public HandlerDelegate<TCommand, TResult> WithPostTransactionDispatching(PostTransactionEventDispatcher postTransactionEventDispatcher) =>
            (command, cancellationToken) =>
            {
                var result = next(command, cancellationToken);
                postTransactionEventDispatcher.Dispatch(result.Events);
                return result;
            };

        public HandlerDelegate<TCommand, TResult> WithReadSideDispatching(ReadSideEventDispatcher readSideEventDispatcher) =>
            (command, cancellationToken) =>
            {
                var result = next(command, cancellationToken);
                readSideEventDispatcher.Dispatch(result.Events);
                return result;
            };
    }
}