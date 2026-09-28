namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Pipeline behavior intercepting command execution.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The command result type.</typeparam>
public interface ICommandPipelineBehavior<TCommand, TResult>
{
    /// <summary>
    /// Handles the command execution within the pipeline.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="nextHandler">The delegate to invoke the next behavior or handler in the pipeline.</param>
    /// <returns>A task returning the list of emitted domain events and the command result.</returns>
    Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> nextHandler);
}
