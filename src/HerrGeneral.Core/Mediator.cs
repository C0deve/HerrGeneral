namespace HerrGeneral;

/// <summary>
/// Mediator implementation
/// </summary>
public class Mediator(IServiceProvider serviceProvider, CommandConcurrencyLimiter limiter)
{
    private static readonly ConcurrentDictionary<Type, ICommandHandlerWrapper<Result>> CommandWrappers = new();
    private static readonly ConcurrentDictionary<(Type CommandType, Type ResultType), object> GenericCommandWrappers = new();

    /// <summary>
    /// Initializes a new instance of the Mediator with a specified maximum concurrent command limit.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="maxConcurrentCommands">The maximum concurrent commands.</param>
    public Mediator(IServiceProvider serviceProvider, int maxConcurrentCommands)
        : this(serviceProvider, new CommandConcurrencyLimiter(maxConcurrentCommands))
    {
    }
    
    /// <summary>
    /// Send a creation command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Result<T>> Send<T>(object command, CancellationToken cancellationToken = default) =>
        LimitConcurrentCommands<Result<T>>(command, async token =>
            {
                var wrapper = (ICommandHandlerWrapper<Result<T>>)GenericCommandWrappers.GetOrAdd(
                    (command.GetType(), typeof(T)),
                    CreateGenericCommandWrapper);

                return await wrapper.Handle(command, serviceProvider, token).ConfigureAwait(false);
            },
            cancellationToken);


    /// <summary>
    /// Send a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Result> Send(object command, CancellationToken cancellationToken = default) =>
        LimitConcurrentCommands<Result>(command, async token =>
            {
                var wrapper = CommandWrappers.GetOrAdd(
                    command.GetType(),
                    CreateCommandWrapper);

                return await wrapper.Handle(command, serviceProvider, token).ConfigureAwait(false);
            },
            cancellationToken);

    private static ICommandHandlerWrapper<Result> CreateCommandWrapper(Type commandType)
    {
        var wrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
        var newExpr = Expression.New(wrapperType);
        var lambda = Expression.Lambda<Func<ICommandHandlerWrapper<Result>>>(newExpr);
        return lambda.Compile()();
    }

    private static object CreateGenericCommandWrapper((Type CommandType, Type ResultType) key)
    {
        var wrapperType = typeof(CommandHandlerWrapper<,>).MakeGenericType(key.CommandType, key.ResultType);
        var newExpr = Expression.New(wrapperType);
        var lambda = Expression.Lambda<Func<object>>(newExpr);
        return lambda.Compile()();
    }

    /// <summary>
    /// Ensures the provided asynchronous function is executed with limited concurrency.
    /// </summary>
    /// <param name="command">The command being executed.</param>
    /// <param name="funcAsync">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the result of the provided asynchronous function.</returns>
    private async Task<TResult> LimitConcurrentCommands<TResult>(object command, Func<CancellationToken, Task<TResult>> funcAsync, CancellationToken cancellationToken)
    {
        using var releaser = await limiter.AcquireAsync(command, cancellationToken).ConfigureAwait(false);
        return await funcAsync(cancellationToken).ConfigureAwait(false);
    }
}
