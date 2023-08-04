using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.WriteSide.Dispatcher;

internal class CreationCommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CreationResult>
    where TCommand : ICommand<CreationResult>
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CreationResult> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCreationResult()
            .WithHandlerLogger(GetLogger(serviceProvider));
}