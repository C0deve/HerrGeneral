using System.Diagnostics;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate Task<TResult> HandlerDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

    public static HandlerDelegate<TCommand, TResult> WithLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger<ICommandHandler<TCommand, TResult>>? logger, CommandLogger commandLogger)
        where TCommand : CommandBase
        where TResult : IWithSuccess
    {
        logger ??= NullLogger<ICommandHandler<TCommand, TResult>>.Instance;
        
        if (logger.IsEnabled(LogLevel.Debug))
            return WithDebugLogger(next, logger, commandLogger);
        return logger.IsEnabled(LogLevel.Information) 
            ? WithInformationLogger(next, logger) 
            : next;
    }

    private static HandlerDelegate<TCommand, TResult> WithDebugLogger<TCommand, TResult>(this HandlerDelegate<TCommand, TResult> next, ILogger<ICommandHandler<TCommand, TResult>> logger, CommandLogger commandLogger)
        where TCommand : CommandBase
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            var watch = new Stopwatch();
            var type = typeof(TCommand).GetFriendlyName();

            var sb = commandLogger.GetStringBuilder(command.Id)
                .StartHandlingCommand(type, command);

            watch.Start();
            try
            {
                return await next(command, cancellationToken);
            }
            catch (EventHandlerDomainException)
            {
                throw;
            }
            catch (DomainException e)
            {
                commandLogger.GetStringBuilder(command.Id).OnException(e, 2);
                throw;
            }
            catch (EventHandlerException)
            {
                throw;
            }
            catch (Exception e)
            {
                commandLogger.GetStringBuilder(command.Id).OnException(e, 2);
                throw;
            }
            finally
            {
                watch.Stop();

                sb.StopHandlingCommand(type, watch.Elapsed);

                logger.LogDebug("{Message}", sb.ToString());
                commandLogger.RemoveStringBuilder(command.Id);
            }
        };


    private static HandlerDelegate<TCommand, TResult> WithInformationLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger logger)
        where TCommand : CommandBase
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            try
            {
                logger.LogInformation("{CommandType}, {Data}", command.GetType(), JsonSerializer.Serialize(command));
                return await next(command, cancellationToken);
            }
            catch (EventHandlerDomainException)
            {
                throw;
            }
            catch (DomainException e)
            {
                logger.LogError(e, null);
                throw;
            }
            catch (EventHandlerException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, null);
                throw;
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithUnitOfWork<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, IUnitOfWork? unitOfWork)
        where TCommand : CommandBase
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            try
            {
                unitOfWork?.Start(command.Id);
                var result = await next(command, cancellationToken);
                unitOfWork?.Commit(command.Id);
                return result;
            }
            catch (Exception)
            {
                unitOfWork?.RollBack(command.Id);
                throw;
            }
            finally
            {
                unitOfWork?.Dispose(command.Id);
            }
        };

    public static HandlerDelegate<TCommand, ChangeResult> WithExceptionToCommandResult<TCommand>(
        this HandlerDelegate<TCommand, ChangeResult> next) =>
        next.WithTryCatch(ChangeResult.PanicFail, ChangeResult.DomainFail);

    public static HandlerDelegate<TCommand, CreateResult> WithExceptionToCreationResult<TCommand>(
        this HandlerDelegate<TCommand, CreateResult> next) =>
        next.WithTryCatch(CreateResult.PanicFail, CreateResult.DomainFail);

    private static HandlerDelegate<TCommand, TResult> WithTryCatch<TCommand, TResult>(this HandlerDelegate<TCommand, TResult> action,
        Func<Exception, TResult> onException,
        Func<DomainError, TResult> onDomainError) =>
        async (command, cancellationToken) =>
        {
            try
            {
                return await action(command, cancellationToken);
            }
            catch (EventHandlerDomainException domainException)
            {
                return onDomainError(domainException.DomainError);
            }
            catch (DomainException domainException)
            {
                return onDomainError(domainException.DomainError);
            }
            catch (EventHandlerException e)
            {
                return onException(e);
            }
            catch (Exception e)
            {
                return onException(e);
            }
        };

    public static HandlerDelegate<TCommand, TResult> WithReadSideDispatching<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ReadSide.IEventDispatcher readSideEventDispatcher)
        where TCommand : CommandBase
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            var result = await next(command, cancellationToken);

            if (result.IsSuccess)
                await readSideEventDispatcher.Dispatch(command.Id, cancellationToken);

            return result;
        };
}