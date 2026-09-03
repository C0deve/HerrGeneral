using System.Collections.Concurrent;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral;

/// <summary>
/// Mediator implementation
/// </summary>
public class Mediator(IServiceProvider serviceProvider, CommandConcurrencyLimiter limiter)
{
    private readonly ConcurrentDictionary<Type, object> _handlerWrappers = new();

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
        LimitConcurrentCommands<Result<T>>(async token =>
            {
                var wrapper = (ICommandHandlerWrapper<Result<T>>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
                {
                    var wrapperType = typeof(CommandHandlerWrapper<,>).MakeGenericType(commandType, typeof(T));
                    return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
                });

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
        LimitConcurrentCommands<Result>(async token =>
            {
                var wrapper = (ICommandHandlerWrapper<Result>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
                {
                    var wrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
                    return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
                });

                return await wrapper.Handle(command, serviceProvider, token).ConfigureAwait(false);
            },
            cancellationToken);

    /// <summary>
    /// Ensures the provided asynchronous function is executed with limited concurrency.
    /// </summary>
    /// <param name="funcAsync">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the result of the provided asynchronous function.</returns>
    private async Task<T> LimitConcurrentCommands<T>(Func<CancellationToken, Task<T>> funcAsync, CancellationToken cancellationToken)
    {
        await limiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await funcAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            limiter.Release();
        }
    }
}