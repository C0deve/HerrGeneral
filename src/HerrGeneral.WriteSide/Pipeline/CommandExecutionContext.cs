namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Execution context for a command running through the pipeline.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The command result type.</typeparam>
public sealed class CommandExecutionContext<TCommand, TResult>
{
    /// <summary>
    /// Gets the command instance being executed.
    /// </summary>
    public TCommand Command { get; }

    /// <summary>
    /// Gets the service provider for resolving scoped services during execution.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the cancellation token for the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the dictionary of custom items associated with this command execution.
    /// </summary>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecutionContext{TCommand, TResult}"/> class.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="serviceProvider">The scoped service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public CommandExecutionContext(TCommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        Command = command;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
    }
}
