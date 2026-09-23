using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HerrGeneral.Core.Diagnostics;
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
                    throw mapper.Map(e,
                        exception => new DomainException(exception),
                        exception => exception);
                }
            };

        public HandlerDelegate<TCommand, TResult> WithTracer(Type handlerType,
            ILogger<ICommandHandler<TCommand, TResult>>? logger,
            ActivityTreeCollector? activityCollector) =>
            (command, cancellationToken) =>
            {
                logger ??= NullLogger<ICommandHandler<TCommand, TResult>>.Instance;

                var commandName = typeof(TCommand).GetFriendlyName();
                var threadId = Environment.CurrentManagedThreadId;

                using var activity = HerrGeneralDiagnostics.StartActivity(
                    HerrGeneralDiagnostics.Activities.ExecuteCommand);

                activity?.SetTag(HerrGeneralDiagnostics.Tags.CommandName, commandName);
                activity?.SetTag(HerrGeneralDiagnostics.Tags.CommandType, typeof(TCommand).ToString());
                activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());
                activity?.SetTag(HerrGeneralDiagnostics.Tags.ThreadId, threadId);

                HerrGeneralDiagnostics.ActiveCommands.Add(1, new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName));

                var watch = Stopwatch.StartNew();
                var status = "Success";

                activityCollector?.StartHandlingCommand(commandName, handlerType, threadId);

                try
                {
                    var result = next(command, cancellationToken);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (EventHandlerDomainException e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    // already logged
                    throw;
                }
                catch (DomainException e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    activityCollector?.OnException(e, 2);
                    throw;
                }
                catch (EventHandlerException e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    // already logged
                    throw;
                }
                catch (System.Exception e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    activityCollector?.OnException(e, 2);
                    throw;
                }
                finally
                {
                    watch.Stop();
                    HerrGeneralDiagnostics.ActiveCommands.Add(-1, new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName));

                    HerrGeneralDiagnostics.CommandsTotal.Add(1,
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));

                    HerrGeneralDiagnostics.CommandsDuration.Record(watch.Elapsed.TotalMilliseconds,
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));

                    activityCollector?.StopHandlingCommand(commandName, watch.Elapsed);

                    if (activityCollector is not null)
                    {
                        logger.LogInformation("{Message}", ActivityTreeFormatter.Format(activityCollector));
                    }
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