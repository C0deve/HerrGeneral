using System.Diagnostics;
using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate (IEnumerable<object> Events, TResult Result) HandlerDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

    public static HandlerDelegate<TCommand, TResult> WithDomainExceptionMapping<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, DomainExceptionMapper mapper) =>
        (command, cancellationToken) =>
        {
            try
            {
                return next(command, cancellationToken);
            }
            catch (Exception e)
            {
                throw mapper.Map(e,
                    exception => new DomainException(exception),
                    exception => exception);
            }
        };
    

    public static HandlerDelegate<TCommand, TResult> WithTracer<TCommand, TResult>(this HandlerDelegate<TCommand, TResult> next,
        Type handlerType,
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
                catch (Exception e)
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

    public static HandlerDelegate<TCommand, TResult> WithUnitOfWork<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, IUnitOfWork? unitOfWork) =>
        (command, cancellationToken) =>
        {
            try
            {
                unitOfWork?.Start();
                var result = next(command, cancellationToken);
                unitOfWork?.Commit();
                return result;
            }
            catch (Exception)
            {
                unitOfWork?.RollBack();
                throw;
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithWriteSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, WriteSideEventDispatcher eventDispatcher) =>
        (command, cancellationToken) =>
        {
            var (events, result) = next(command, cancellationToken);
            return (eventDispatcher.Dispatch(events), result);
        };

    public static HandlerDelegate<TCommand, TResult> WithReadSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ReadSideEventDispatcher readSideEventDispatcher) =>
        (command, cancellationToken) =>
        {
            var result = next(command, cancellationToken);
            readSideEventDispatcher.Dispatch(result.Events);
            return result;
        };
}