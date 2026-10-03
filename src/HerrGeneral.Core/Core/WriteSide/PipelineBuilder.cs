using HerrGeneral.Core.Diagnostics;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.WriteSide.Behaviors;
using HerrGeneral.WriteSide.Pipeline;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Core.WriteSide;

internal static class PipelineBuilder
{
    private static readonly ConcurrentDictionary<Type, Type?> HandlerTypeCache = new();

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

        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();
        var writeSideEventDispatcher = serviceProvider.GetRequiredService<WriteSideEventDispatcher>();
        var transactionalProjectionDispatcher = serviceProvider.GetRequiredService<TransactionalProjectionEventDispatcher>();
        var postTransactionDispatcher = serviceProvider.GetRequiredService<PostTransactionEventDispatcher>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
        var collector = serviceProvider.GetService<ActivityTreeCollector>();
        var logger = serviceProvider.GetService<ILogger<ICommandHandler<TCommand, TResult>>>();

        // Resolve handler type for tracing; handlers implementing IHandlerTypeProvider are resolved dynamically per instance.
        var handlerType = HandlerTypeCache.GetOrAdd(
            commandHandler.GetType(),
            static t => typeof(IHandlerTypeProvider).IsAssignableFrom(t)
                ? null
                : t);

        if (handlerType is null && commandHandler is IHandlerTypeProvider handlerTypeProvider)
        {
            handlerType = handlerTypeProvider.GetHandlerType();
        }
        else
        {
            handlerType ??= commandHandler.GetType();
        }

        var tracingBehavior = new TracingBehavior<TCommand, TResult>(handlerType, logger, collector);
        var postTxBehavior = new PostTransactionBehavior<TCommand, TResult>(postTransactionDispatcher);
        var uowBehavior = new UnitOfWorkBehavior<TCommand, TResult>(unitOfWork, collector);
        var txProjBehavior = new TransactionalProjectionBehavior<TCommand, TResult>(transactionalProjectionDispatcher);
        var writeSideBehavior = new WriteSideDispatchingBehavior<TCommand, TResult>(writeSideEventDispatcher);
        var domainExBehavior = new DomainExceptionMappingBehavior<TCommand, TResult>(domainExceptionMapper);

        // Fast path: when no custom behaviors are registered, construct the direct delegate chain without array allocation or sorting
        if (customBehaviors is ICollection<ICommandPipelineBehavior<TCommand, TResult>> { Count: 0 } or null)
        {
            CommandHandlerDelegate<TResult> step6 = () => domainExBehavior.HandleAsync(context, current);
            CommandHandlerDelegate<TResult> step5 = () => writeSideBehavior.HandleAsync(context, step6);
            CommandHandlerDelegate<TResult> step4 = () => txProjBehavior.HandleAsync(context, step5);
            CommandHandlerDelegate<TResult> step3 = () => uowBehavior.HandleAsync(context, step4);
            CommandHandlerDelegate<TResult> step2 = () => postTxBehavior.HandleAsync(context, step3);
            return () => tracingBehavior.HandleAsync(context, step2);
        }

        var customArray = customBehaviors as ICommandPipelineBehavior<TCommand, TResult>[] ?? customBehaviors.ToArray();
        if (customArray.Length == 0)
        {
            CommandHandlerDelegate<TResult> step6 = () => domainExBehavior.HandleAsync(context, current);
            CommandHandlerDelegate<TResult> step5 = () => writeSideBehavior.HandleAsync(context, step6);
            CommandHandlerDelegate<TResult> step4 = () => txProjBehavior.HandleAsync(context, step5);
            CommandHandlerDelegate<TResult> step3 = () => uowBehavior.HandleAsync(context, step4);
            CommandHandlerDelegate<TResult> step2 = () => postTxBehavior.HandleAsync(context, step3);
            return () => tracingBehavior.HandleAsync(context, step2);
        }

        // Full path: combine core and custom behaviors, sort by phase and order, then compose the pipeline
        var allBehaviors = new ICommandPipelineBehavior<TCommand, TResult>[6 + customArray.Length];
        allBehaviors[0] = tracingBehavior;
        allBehaviors[1] = postTxBehavior;
        allBehaviors[2] = uowBehavior;
        allBehaviors[3] = txProjBehavior;
        allBehaviors[4] = writeSideBehavior;
        allBehaviors[5] = domainExBehavior;
        Array.Copy(customArray, 0, allBehaviors, 6, customArray.Length);
        Array.Sort(allBehaviors, BehaviorComparer<TCommand, TResult>.Instance);

        // Wrap behaviors from innermost to outermost (reverse traversal)
        for (var i = allBehaviors.Length - 1; i >= 0; i--)
        {
            var behavior = allBehaviors[i];
            var next = current;
            current = () => behavior.HandleAsync(context, next);
        }

        return current;
    }

    private sealed class BehaviorComparer<TCommand, TResult> : IComparer<ICommandPipelineBehavior<TCommand, TResult>>
    {
        public static readonly BehaviorComparer<TCommand, TResult> Instance = new();

        public int Compare(ICommandPipelineBehavior<TCommand, TResult>? x, ICommandPipelineBehavior<TCommand, TResult>? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var phaseX = x is IOrderedPipelineBehavior ox ? ox.Phase : PipelinePhase.Core;
            var phaseY = y is IOrderedPipelineBehavior oy ? oy.Phase : PipelinePhase.Core;

            var phaseComparison = phaseX.CompareTo(phaseY);
            if (phaseComparison != 0) return phaseComparison;

            var orderX = x is IOrderedPipelineBehavior ox2 ? ox2.OrderWithinPhase : 0;
            var orderY = y is IOrderedPipelineBehavior oy2 ? oy2.OrderWithinPhase : 0;

            return orderX.CompareTo(orderY);
        }
    }
}
