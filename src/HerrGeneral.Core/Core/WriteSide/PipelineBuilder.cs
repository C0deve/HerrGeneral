using HerrGeneral.Core.Diagnostics;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.WriteSide.Behaviors;
using HerrGeneral.WriteSide.Pipeline;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal static class PipelineBuilder
{
    public static CommandHandlerDelegate<TResult> Build<TCommand, TResult>(
        IServiceProvider serviceProvider,
        CommandExecutionContext<TCommand, TResult> context)
    {
        var commandHandler = serviceProvider.GetService<ICommandHandler<TCommand, TResult>>()
            ?? throw new MissingCommandHandlerRegistrationException(typeof(TCommand));

        // Innermost terminal step: execute the command handler
        CommandHandlerDelegate<TResult> current = () => Task.FromResult(commandHandler.Handle(context.Command));

        // Discover custom behaviors registered in the DI container
        var customBehaviors = serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TResult>>();

        // Discover built-in standard pipeline behaviors
        var builtInBehaviors = GetBuiltInBehaviors(serviceProvider, commandHandler);

        // Combine and order all behaviors by phase and order within phase
        var allBehaviors = builtInBehaviors
            .Concat(customBehaviors)
            .OrderBy(GetPhase)
            .ThenBy(GetOrderWithinPhase)
            .ToList();

        // Wrap behaviors from innermost to outermost (reverse traversal)
        for (var i = allBehaviors.Count - 1; i >= 0; i--)
        {
            var behavior = allBehaviors[i];
            var next = current;
            current = () => behavior.HandleAsync(context, next);
        }

        return current;
    }

    private static PipelinePhase GetPhase(object behavior) =>
        behavior is IOrderedPipelineBehavior ordered ? ordered.Phase : PipelinePhase.Core;

    private static int GetOrderWithinPhase(object behavior) =>
        behavior is IOrderedPipelineBehavior ordered ? ordered.OrderWithinPhase : 0;

    private static IEnumerable<ICommandPipelineBehavior<TCommand, TResult>> GetBuiltInBehaviors<TCommand, TResult>(
        IServiceProvider serviceProvider,
        ICommandHandler<TCommand, TResult> commandHandler)
    {
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();
        var writeSideEventDispatcher = serviceProvider.GetRequiredService<WriteSideEventDispatcher>();
        var transactionalProjectionDispatcher = serviceProvider.GetRequiredService<TransactionalProjectionEventDispatcher>();
        var postTransactionDispatcher = serviceProvider.GetRequiredService<PostTransactionEventDispatcher>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        var collector = serviceProvider.GetService<ActivityTreeCollector>();
        var logger = serviceProvider.GetService<ILogger<ICommandHandler<TCommand, TResult>>>();

        var handlerType = commandHandler is IHandlerTypeProvider handlerTypeProvider
            ? handlerTypeProvider.GetHandlerType()
            : commandHandler.GetType();

        yield return new DomainExceptionMappingBehavior<TCommand, TResult>(domainExceptionMapper);
        yield return new WriteSideDispatchingBehavior<TCommand, TResult>(writeSideEventDispatcher);
        yield return new TransactionalProjectionBehavior<TCommand, TResult>(transactionalProjectionDispatcher);
        yield return new UnitOfWorkBehavior<TCommand, TResult>(unitOfWork, collector);
        yield return new PostTransactionBehavior<TCommand, TResult>(postTransactionDispatcher);
        yield return new TracingBehavior<TCommand, TResult>(handlerType, logger, collector);
    }
}
