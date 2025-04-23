using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, Result>
{
    public override Task<Result> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<Unit>(serviceProvider))(UnitOfWorkId.New(), (TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, Result> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, Unit> next) =>
        (operationId, command, cancellationToken) =>
            Task.Run(() =>
            {
                try
                {
                    _ = next(operationId, command, cancellationToken);
                    return Result.Success();
                }
                catch (EventHandlerDomainException domainException)
                {
                    return Result.DomainFail(domainException.DomainError);
                }
                catch (DomainException domainException)
                {
                    return Result.DomainFail(domainException.DomainError);
                }
                catch (EventHandlerException e)
                {
                    return Result.PanicFail(e);
                }
                catch (Exception e)
                {
                    return Result.PanicFail(e);
                }
            }, cancellationToken);
}

internal class CommandHandlerWrapper<TCommand, TResult> : CommandHandlerWrapperBase<TCommand, Result<TResult>>
{
    public override Task<Result<TResult>> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<TResult>(serviceProvider))(UnitOfWorkId.New(), (TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, Result<TResult>> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, TResult> next) =>
        (operationId, command, cancellationToken) =>
            Task.Run(() =>
            {
                try
                {
                    var result = next(operationId, command, cancellationToken);
                    return Result.Success(result.Result);
                }
                catch (EventHandlerDomainException domainException)
                {
                    return Result<TResult>.DomainFail(domainException.DomainError);
                }
                catch (DomainException domainException)
                {
                    return Result<TResult>.DomainFail(domainException.DomainError);
                }
                catch (EventHandlerException e)
                {
                    return Result<TResult>.PanicFail(e);
                }
                catch (Exception e)
                {
                    return Result<TResult>.PanicFail(e);
                }
            }, cancellationToken);
}