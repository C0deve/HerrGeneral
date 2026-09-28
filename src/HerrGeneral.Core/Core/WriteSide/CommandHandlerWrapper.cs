using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Core.WriteSide;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<Result>
{
    public override async Task<Result> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var context = new CommandExecutionContext<TCommand, Unit>((TCommand)command, serviceProvider, cancellationToken);
        var pipeline = PipelineBuilder.Build(serviceProvider, context);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = await pipeline().ConfigureAwait(false);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (EventHandlerDomainException domainException)
        {
            return Result.DomainFail(domainException.InnerException!);
        }
        catch (DomainException domainException)
        {
            return Result.DomainFail(domainException.InnerException!);
        }
        catch (EventHandlerException e)
        {
            return Result.PanicFail(e);
        }
        catch (System.Exception e)
        {
            return Result.PanicFail(e);
        }
    }
}

internal class CommandHandlerWrapper<TCommand, TResult> : CommandHandlerWrapperBase<Result<TResult>>
{
    public override async Task<Result<TResult>> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var context = new CommandExecutionContext<TCommand, TResult>((TCommand)command, serviceProvider, cancellationToken);
        var pipeline = PipelineBuilder.Build(serviceProvider, context);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await pipeline().ConfigureAwait(false);
            return Result<TResult>.Success(result.Result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (EventHandlerDomainException domainException)
        {
            return Result<TResult>.DomainFail(domainException.InnerException!);
        }
        catch (DomainException domainException)
        {
            return Result<TResult>.DomainFail(domainException.InnerException!);
        }
        catch (EventHandlerException e)
        {
            return Result<TResult>.PanicFail(e);
        }
        catch (System.Exception e)
        {
            return Result<TResult>.PanicFail(e);
        }
    }
}