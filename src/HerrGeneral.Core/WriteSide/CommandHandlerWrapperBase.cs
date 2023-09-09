using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal abstract class CommandHandlerWrapperBase<TCommand, TResult> : ICommandHandlerWrapper<TResult>
    where TCommand : CommandBase<TResult> 
    where TResult : IWithSuccess
{
    public async Task<TResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => await Handle((TCommand)command, serviceProvider, cancellationToken);

    private async Task<TResult> Handle(TCommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        await BuildPipeline(serviceProvider)(command, cancellationToken);

    protected virtual CommandPipeline.HandlerDelegate<TCommand, TResult> BuildPipeline(IServiceProvider serviceProvider)
    {
        var commandHandler = GetHandler(serviceProvider);
        var logger = GetLogger(serviceProvider);
        var readSideEventDispatcher = serviceProvider.GetRequiredService<ReadSide.IEventDispatcher>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        var commandLogger = serviceProvider.GetRequiredService<CommandLogger>();
        
        return
            Start(commandHandler)
                .WithReadSideDispatching(readSideEventDispatcher)
                .WithUnitOfWork(unitOfWork)
                .WithErrorLogger(logger, commandLogger);
    }

    private static CommandPipeline.HandlerDelegate<TCommand, TResult> Start(ICommandHandler<TCommand, TResult> commandHandler) =>
        commandHandler.Handle;

    private static ICommandHandler<TCommand, TResult> GetHandler(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ICommandHandler<TCommand, TResult>>() ?? throw new MissingCommandHandlerRegistrationException(typeof(TCommand));

    protected static ILogger<ICommandHandler<TCommand, TResult>>? GetLogger(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ILogger<ICommandHandler<TCommand, TResult>>>();
}