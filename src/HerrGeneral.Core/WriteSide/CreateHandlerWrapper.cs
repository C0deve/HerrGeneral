using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class CreateHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CreateResult>
    where TCommand : Create
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CreateResult> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCreationResult();
}