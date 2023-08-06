using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide.Dispatcher;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CommandResultV2> where TCommand : ICommand<CommandResultV2>
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CommandResultV2> BuildPipeline(IServiceProvider serviceProvider) =>
        base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCommandResult()
            .WithHandlerLogger(GetLogger(serviceProvider));
}