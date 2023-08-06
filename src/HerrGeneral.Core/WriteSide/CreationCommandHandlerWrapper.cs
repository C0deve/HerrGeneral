using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class CreationCommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CreationResult>
    where TCommand : CreationCommand
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CreationResult> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCreationResult()
            .WithHandlerLogger(GetLogger(serviceProvider));
}