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
    public async Task<CreateResult> Send(Create command, CancellationToken cancellationToken = default)
    {
        var wrapper = (ICommandHandlerWrapper<CreateResult>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = typeof(CreateHandlerWrapper<>).MakeGenericType(commandType);
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
    public async Task<ChangeResult> Send(Change command, CancellationToken cancellationToken = default)
    {
        var wrapper = (ICommandHandlerWrapper<ChangeResult>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = typeof(ChangeHandlerWrapper<>).MakeGenericType(commandType);
            return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
        });

        return await wrapper.Handle(command, _serviceProvider, cancellationToken).ConfigureAwait(false);
    }
}