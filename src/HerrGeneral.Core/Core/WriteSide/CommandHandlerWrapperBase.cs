namespace HerrGeneral.Core.WriteSide;

internal abstract class CommandHandlerWrapperBase<TResult> : ICommandHandlerWrapper<TResult>
{
    public abstract Task<TResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}