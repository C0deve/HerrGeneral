using System.Diagnostics;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Logger;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide;

internal static class CommandPipeline
{
    public delegate Task<TResult> HandlerDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

    public static HandlerDelegate<TCommand, TResult> WithHandlerLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger<ICommandHandler<TCommand, TResult>>? logger)
        where TCommand : CommandBase<TResult>
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            logger ??= NullLogger<ICommandHandler<TCommand, TResult>>.Instance;
            var watch = new Stopwatch();
            var type = typeof(TCommand).EvaluateType();

            logger.StartHandling(type, command);

            watch.Start();
            var result = await next(command, cancellationToken);
            watch.Stop();

            logger.StopHandling(type, watch.Elapsed);

            return result;
        };


    public static HandlerDelegate<TCommand, TResult> WithErrorLogger<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, ILogger? logger) 
        where TCommand : CommandBase<TResult> 
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            logger ??= NullLogger.Instance;

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
                logger.Log(e);
                throw;
            }
            catch (EventHandlerException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw;
            }
        };


    public static HandlerDelegate<TCommand, TResult> WithUnitOfWork<TCommand, TResult>(
        this HandlerDelegate<TCommand, TResult> next, IUnitOfWork? unitOfWork) 
        where TCommand : CommandBase<TResult> 
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


    public static HandlerDelegate<TCommand, CommandResultV2> WithExceptionToCommandResult<TCommand>(
        this HandlerDelegate<TCommand, CommandResultV2> next) =>
        next.WithTryCatch(CommandResultV2.PanicFail, CommandResultV2.DomainFail);

    public static HandlerDelegate<TCommand, CreationResult> WithExceptionToCreationResult<TCommand>(
        this HandlerDelegate<TCommand, CreationResult> next) =>
        next.WithTryCatch(CreationResult.PanicFail, CreationResult.DomainFail);

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
        where TCommand : CommandBase<TResult>
        where TResult : IWithSuccess =>
        async (command, cancellationToken) =>
        {
            var result = await next(command, cancellationToken);

            if (result.IsSuccess)
                await readSideEventDispatcher.Dispatch(command.Id, cancellationToken);

            return result;
        };
}