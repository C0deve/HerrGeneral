using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.WriteSide;

internal class CreateHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CreateResult>
    where TCommand : Create
{
    protected override CommandPipeline.HandlerDelegate<TCommand, CreateResult> BuildPipeline(IServiceProvider serviceProvider)
    {
        var commandLogger = serviceProvider.GetRequiredService<CommandLogger>();

        return base
            .BuildPipeline(serviceProvider)
            .WithExceptionToCreationResult()
            .WithHandlerLogger(GetLogger(serviceProvider), commandLogger);
    }
}