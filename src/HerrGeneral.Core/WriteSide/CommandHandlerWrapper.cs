using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CommandResult> where TCommand : Command
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CommandResult> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCommandResult()
            .WithHandlerLogger(GetLogger(serviceProvider));
}