using System.Collections.Concurrent;
using HerrGeneral.Core.WriteSide.Dispatcher;
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
    public async Task<CreationResult> Send(CreationCommand command, CancellationToken cancellationToken = default) =>
        await Send(command, typeof(CreationCommandHandlerWrapper<>), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Send a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CommandResultV2> Send(Command command, CancellationToken cancellationToken = default) =>
        await Send(command, typeof(CommandHandlerWrapper<>), cancellationToken).ConfigureAwait(false);

    private async Task<TResult> Send<TResult>(CommandBase<TResult> command, Type openWrapperType, CancellationToken cancellationToken) 
        where TResult : IWithSuccess
    {
        var wrapper = (ICommandHandlerWrapper<TResult>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = openWrapperType.MakeGenericType(commandType);
            return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {commandType}");
        });

        return await wrapper.Handle(command, _serviceProvider, cancellationToken);
    }
}