using System.Diagnostics;
using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
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
            (command, cancellationToken) =>
            {
                var watch = new Stopwatch();
                var commandType = typeof(TCommand).GetFriendlyName();

                commandLogger
                    .StartHandlingCommand(commandType);

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
                    commandLogger.OnException(e, 2);
                    throw;
                }
                catch (EventHandlerException)
                {
                    // already logged
                    throw;
                }
                catch (Exception e)
                {
                    commandLogger.OnException(e, 2);
                    throw;
                }
                finally
                {
                    watch.Stop();

                    commandLogger.StopHandlingCommand(commandType, watch.Elapsed);

                    logger.LogDebug("{Message}", commandLogger.BuildString());
                }
            };

    private static HandlerDelegate<TCommand, TResult> WithInformationLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger logger) =>
        (command, cancellationToken) =>
        {
            try
            {
                logger.LogInformation("{CommandType}, {Data}", command?.GetType(), JsonSerializer.Serialize(command));
                return next(command, cancellationToken);
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
            finally
            {
                unitOfWork?.Dispose();
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithWriteSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, WriteSideEventDispatcher eventDispatcher) =>
        (command, cancellationToken) =>
        {
            var (events, result) = next(command, cancellationToken);
            var enumerable = events as object[] ?? events.ToArray();
            foreach (var @event in enumerable)
                eventDispatcher.Dispatch(@event);
            return (enumerable, result);
        };

    public static HandlerDelegate<TCommand, TResult> WithReadSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ReadSideEventDispatcher readSideEventDispatcher) =>
        (command, cancellationToken) =>
        {
            var result = next(command, cancellationToken);
            readSideEventDispatcher.Dispatch();
            return result;
        };
}