using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.WriteSide;

internal class CommandHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CommandResult> where TCommand : Command
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CommandResult> BuildPipeline(IServiceProvider serviceProvider)
    {
        var commandLogger = serviceProvider.GetRequiredService<CommandLogger>();

        return base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCommandResult()
            .WithHandlerLogger(GetLogger(serviceProvider), commandLogger);
    }
}