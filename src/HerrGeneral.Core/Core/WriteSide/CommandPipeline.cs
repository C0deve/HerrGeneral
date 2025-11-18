using System.Diagnostics;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Exception;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate (IEnumerable<object> Events, TResult Result) HandlerDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

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
            CommandExecutionTracer? commandExecutionTracer) =>
            commandExecutionTracer is null
                ? next
                : (command, cancellationToken) =>
                {
                    logger ??= NullLogger<ICommandHandler<TCommand, TResult>>.Instance;

                    var watch = new Stopwatch();
                    var commandType = typeof(TCommand).GetFriendlyName();

                    commandExecutionTracer
                        .StartHandlingCommand(commandType, handlerType);

                    watch.Start();
                    try
                    {
                        return next(command, cancellationToken);
                    }
                    catch (EventHandlerDomainException)
                    {
                        // already logged
                        throw;
                    }
                    catch (DomainException e)
                    {
                        commandExecutionTracer.OnException(e, 2);
                        throw;
                    }
                    catch (EventHandlerException)
                    {
                        // already logged
                        throw;
                    }
                    catch (System.Exception e)
                    {
                        commandExecutionTracer.OnException(e, 2);
                        throw;
                    }
                    finally
                    {
                        watch.Stop();

                        commandExecutionTracer.StopHandlingCommand(commandType, watch.Elapsed);

                        logger.LogInformation("{Message}", commandExecutionTracer.BuildString());
                    }
                };

        public HandlerDelegate<TCommand, TResult> WithUnitOfWork(IUnitOfWork? unitOfWork) =>
            (command, cancellationToken) =>
            {
                try
                {
                    unitOfWork?.Start();
                    var result = next(command, cancellationToken);
                    unitOfWork?.Commit();
                    return result;
                }
                catch (System.Exception)
                {
                    unitOfWork?.RollBack();
                    throw;
                }
            };

        public HandlerDelegate<TCommand, TResult> WithWriteSideDispatching(WriteSideEventDispatcher eventDispatcher) =>
            (command, cancellationToken) =>
            {
                var (events, result) = next(command, cancellationToken);
                return (eventDispatcher.Dispatch(events), result);
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