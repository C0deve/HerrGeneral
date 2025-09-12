using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.WriteSide.Tracer;
using HerrGeneral.Exception;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal delegate Task<TResult> HandlerWrapperDelegate<in TCommand, TResult>(TCommand command, CancellationToken cancellationToken);

internal abstract class CommandHandlerWrapperBase<TCommand, TResult> : ICommandHandlerWrapper<TResult>
{
    public abstract Task<TResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken);

    protected static CommandPipeline.HandlerDelegate<TCommand, TReturn> BuildPipeline<TReturn>(IServiceProvider serviceProvider)
    {
        var commandHandler = GetHandler<TReturn>(serviceProvider);
        var logger = GetLogger<TReturn>(serviceProvider);
        var writeSideEventDispatcher = serviceProvider.GetRequiredService<WriteSideEventDispatcher>();
        var readSideEventDispatcher = serviceProvider.GetRequiredService<ReadSideEventDispatcher>();
        var tracer = serviceProvider.GetService<CommandExecutionTracer>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        if (unitOfWork is not null && tracer is not null)
            unitOfWork = new UnitOfWorkTraceDecorator(unitOfWork, tracer);
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();
        var handlerType = commandHandler is IHandlerTypeProvider handlerTypeProvider
            ? handlerTypeProvider.GetHandlerType()
            : commandHandler.GetType();
        return
            Start(commandHandler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithWriteSideDispatching(writeSideEventDispatcher)
                .WithReadSideDispatching(readSideEventDispatcher)
                .WithUnitOfWork(unitOfWork)
                .WithTracer(handlerType, logger, tracer);
    }

    private static CommandPipeline.HandlerDelegate<TCommand, TReturn> Start<TReturn>(ICommandHandler<TCommand, TReturn> commandHandler) =>
        (command, _) => commandHandler.Handle(command);

    private static ICommandHandler<TCommand, T> GetHandler<T>(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ICommandHandler<TCommand, T>>() ??
        throw new MissingCommandHandlerRegistrationException(typeof(TCommand));

    private static ILogger<ICommandHandler<TCommand, T>>? GetLogger<T>(IServiceProvider serviceProvider) =>
        serviceProvider.GetService<ILogger<ICommandHandler<TCommand, T>>>();
}