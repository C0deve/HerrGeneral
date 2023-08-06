namespace HerrGeneral.Core.WriteSide.Dispatcher;

internal interface ICommandHandlerWrapper<TResult>
{
    Task<TResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}