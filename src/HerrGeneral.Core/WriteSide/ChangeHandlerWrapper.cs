using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class ChangeHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, ChangeResult> where TCommand : Change
{
    protected override CommandPipeline.HandlerDelegate<TCommand, ChangeResult> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCommandResult();
}