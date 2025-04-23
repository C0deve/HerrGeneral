using System.Collections.Concurrent;
using HerrGeneral.Core.WriteSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core;

/// <summary>
/// Mediator implementation
/// </summary>
public class Mediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _handlerWrappers = new();

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public Mediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Send a creation command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<T>> Send<T>(object command, CancellationToken cancellationToken = default)
    {
        var wrapper = (ICommandHandlerWrapper<Result<T>>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = typeof(CommandHandlerWrapper<,>).MakeGenericType(commandType, typeof(T));
            return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
        });

        return await wrapper.Handle(command, _serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Send a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> Send(object command, CancellationToken cancellationToken = default)
    {
        var wrapper = (ICommandHandlerWrapper<Result>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(commandType);
            return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
        });

        return await wrapper.Handle(command, _serviceProvider, cancellationToken).ConfigureAwait(false);
    }
}