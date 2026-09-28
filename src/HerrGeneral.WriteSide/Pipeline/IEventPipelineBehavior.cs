namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Delegate representing the next handler or behavior in the event processing pipeline.
/// </summary>
/// <returns>A task returning the list of emitted cascading events.</returns>
public delegate Task<IReadOnlyList<object>> EventHandlerDelegate();

/// <summary>
/// Pipeline behavior intercepting event handling and dispatching.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
// ReSharper disable once UnusedType.Global
public interface IEventPipelineBehavior<in TEvent>
{
    /// <summary>
    /// Handles the event execution within the pipeline.
    /// </summary>
    /// <param name="event">The event being processed.</param>
    /// <param name="serviceProvider">The scoped service provider.</param>
    /// <param name="next">The delegate to invoke the next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task returning the list of emitted cascading domain events.</returns>
    // ReSharper disable once UnusedMember.Global
    Task<IReadOnlyList<object>> HandleAsync(
        TEvent @event,
        IServiceProvider serviceProvider,
        EventHandlerDelegate next,
        CancellationToken cancellationToken = default);
}
