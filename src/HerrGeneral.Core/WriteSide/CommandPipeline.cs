using System.Diagnostics;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate (IEnumerable<object> Events, TResult Result) HandlerDelegate<in TCommand, TResult>(UnitOfWorkId operationId, TCommand command, CancellationToken cancellationToken);

    public static HandlerDelegate<TCommand, TResult> WithDomainExceptionMapping<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, DomainExceptionMapper mapper) =>
        (operationId, command, cancellationToken) =>
        {
            try
            {
                return next(operationId, command, cancellationToken);
            }
            catch (Exception e)
            {
                throw mapper.Map(e,
                    exception => new DomainException(exception),
                    exception => exception);
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger<ICommandHandler<TCommand, TResult>>? logger, CommandLogger commandLogger)
    {
        logger ??= NullLogger<ICommandHandler<TCommand, TResult>>.Instance;

        if (logger.IsEnabled(LogLevel.Debug))
            return WithDebugLogger(next, logger, commandLogger);
        return logger.IsEnabled(LogLevel.Information)
            ? WithInformationLogger(next, logger)
            : next;
    }

    private static HandlerDelegate<TCommand, TResult> WithDebugLogger<TCommand, TResult>(this HandlerDelegate<TCommand, TResult> next, ILogger<ICommandHandler<TCommand, TResult>> logger, CommandLogger commandLogger)
        =>
            (operationId, command, cancellationToken) =>
            {
                var watch = new Stopwatch();
                var commandType = typeof(TCommand).GetFriendlyName();

                var sb = commandLogger.GetStringBuilder(operationId)
                    .StartHandlingCommand(commandType, operationId);

                watch.Start();
                try
                {
                    return next(operationId, command, cancellationToken);
                }
                catch (EventHandlerDomainException)
                {
                    // already logged
                    throw;
                }
                catch (DomainException e)
                {
                    commandLogger.GetStringBuilder(operationId).OnException(e, 2);
                    throw;
                }
                catch (EventHandlerException)
                {
                    // already logged
                    throw;
                }
                catch (Exception e)
                {
                    commandLogger.GetStringBuilder(operationId).OnException(e, 2);
                    throw;
                }
                finally
                {
                    watch.Stop();

                    sb.StopHandlingCommand(commandType, watch.Elapsed);

                    logger.LogDebug("{Message}", sb.ToString());
                    commandLogger.RemoveStringBuilder(operationId);
                }
            };

    private static HandlerDelegate<TCommand, TResult> WithInformationLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger logger) =>
        (operationId, command, cancellationToken) =>
        {
            try
            {
                logger.LogInformation("{CommandType}, {Data}", command?.GetType(), JsonSerializer.Serialize(command));
                return next(operationId, command, cancellationToken);
            }
            catch (EventHandlerDomainException)
            {
                // already logged
                throw;
            }
            catch (DomainException e)
            {
                logger.LogError(e, null);
                throw;
            }
            catch (EventHandlerException)
            {
                // already logged
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, null);
                throw;
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithUnitOfWork<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, IUnitOfWork? unitOfWork) =>
        (operationId, command, cancellationToken) =>
        {
            try
            {
                unitOfWork?.Start(operationId);
                var result = next(operationId, command, cancellationToken);
                unitOfWork?.Commit(operationId);
                return result;
            }
            catch (Exception)
            {
                unitOfWork?.RollBack(operationId);
                throw;
            }
            finally
            {
                unitOfWork?.Dispose(operationId);
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithWriteSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, WriteSideEventDispatcher eventDispatcher) =>
        (operationId, command, cancellationToken) =>
        {
            var (events, result) = next(operationId, command, cancellationToken);
            var enumerable = events as object[] ?? events.ToArray();
            foreach (var @event in enumerable)
                eventDispatcher.Dispatch(operationId, @event, CancellationToken.None);
            return (enumerable, result);
        };

    public static HandlerDelegate<TCommand, TResult> WithReadSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ReadSideEventDispatcher readSideEventDispatcher) =>
        (operationId, command, cancellationToken) =>
        {
            var result = next(operationId, command, cancellationToken);
            readSideEventDispatcher.Dispatch(operationId, cancellationToken);
            return result;
        };
}