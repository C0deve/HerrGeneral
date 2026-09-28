namespace HerrGeneral.WriteSide.Pipeline;

/// <summary>
/// Delegate representing the next handler or behavior in the execution pipeline.
/// </summary>
/// <typeparam name="TResult">The command result type.</typeparam>
/// <returns>A task returning the list of emitted domain events and the command result.</returns>
public delegate Task<(IReadOnlyList<object> Events, TResult Result)> CommandHandlerDelegate<TResult>();
