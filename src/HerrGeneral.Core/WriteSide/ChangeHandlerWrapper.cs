using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.WriteSide;

internal class ChangeHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, ChangeResult> where TCommand : Change
{
    protected override CommandPipeline.HandlerDelegate<TCommand, ChangeResult> BuildPipeline(IServiceProvider serviceProvider)
    {
        var commandLogger = serviceProvider.GetRequiredService<CommandLogger>();

        return base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCommandResult()
            .WithHandlerLogger(GetLogger(serviceProvider), commandLogger);
    }
}