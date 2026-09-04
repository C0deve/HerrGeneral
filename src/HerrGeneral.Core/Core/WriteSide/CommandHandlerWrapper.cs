namespace HerrGeneral.Core.WriteSide;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, Result>
{
    public override Task<Result> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<Unit>(serviceProvider))((TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, Result> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, Unit> next) =>
        (command, cancellationToken) =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                _ = next(command, cancellationToken);
                return Task.FromResult(Result.Success());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (EventHandlerDomainException domainException)
            {
                return Task.FromResult(Result.DomainFail(domainException.InnerException!));
            }
            catch (DomainException domainException)
            {
                return Task.FromResult(Result.DomainFail(domainException.InnerException!));
            }
            catch (EventHandlerException e)
            {
                return Task.FromResult(Result.PanicFail(e));
            }
            catch (System.Exception e)
            {
                return Task.FromResult(Result.PanicFail(e));
            }
        };
}

internal class CommandHandlerWrapper<TCommand, TResult> : CommandHandlerWrapperBase<TCommand, Result<TResult>>
{
    public override Task<Result<TResult>> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<TResult>(serviceProvider))((TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, Result<TResult>> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, TResult> next) =>
        (command, cancellationToken) =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = next(command, cancellationToken);
                return Task.FromResult(Result.Success(result.Result));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (EventHandlerDomainException domainException)
            {
                return Task.FromResult(Result<TResult>.DomainFail(domainException.InnerException!));
            }
            catch (DomainException domainException)
            {
                return Task.FromResult(Result<TResult>.DomainFail(domainException.InnerException!));
            }
            catch (EventHandlerException e)
            {
                return Task.FromResult(Result<TResult>.PanicFail(e));
            }
            catch (System.Exception e)
            {
                return Task.FromResult(Result<TResult>.PanicFail(e));
            }
        };
}