using HerrGeneral.Core.Error;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal delegate Task<TResult> HandlerWrapperDelegate<in TCommand, TResult>(UnitOfWorkId operationId, TCommand command, CancellationToken cancellationToken);

internal abstract class CommandHandlerWrapperBase<TCommand, TResult> : ICommandHandlerWrapper<TResult>
{
    public abstract Task<TResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken);
    
    protected static CommandPipeline.HandlerDelegate<TCommand, TReturn> BuildPipeline<TReturn>(IServiceProvider serviceProvider)
    {
        var commandHandler = GetHandler<TReturn>(serviceProvider);
        var logger = GetLogger<TReturn>(serviceProvider);
        var writeSideEventDispatcher = serviceProvider.GetRequiredService<WriteSideEventDispatcher>();
        var readSideEventDispatcher = serviceProvider.GetRequiredService<ReadSideEventDispatcher>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        var commandLogger = serviceProvider.GetRequiredService<CommandLogger>();
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();
        
        return
            Start(commandHandler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithWriteSideDispatching(writeSideEventDispatcher)
                .WithReadSideDispatching(readSideEventDispatcher)
                .WithUnitOfWork(unitOfWork)
                .WithLogger(logger, commandLogger);
    }

    private static CommandPipeline.HandlerDelegate<TCommand, TReturn> Start<TReturn>(ICommandHandler<TCommand, TReturn> commandHandler) =>
        (_, command, token) => commandHandler.Handle(command);

    private static ICommandHandler<TCommand, T> GetHandler<T>(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ICommandHandler<TCommand, T>>() ?? 
        throw new MissingCommandHandlerRegistrationException(typeof(TCommand));

    private static ILogger<ICommandHandler<TCommand, T>>? GetLogger<T>(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ILogger<ICommandHandler<TCommand, T>>>();
}