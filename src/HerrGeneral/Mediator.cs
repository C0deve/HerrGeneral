using System.Collections.Concurrent;
using HerrGeneral.Contracts;
using HerrGeneral.Contracts.WriteSide;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.Dispatcher;

namespace HerrGeneral;

public class Mediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _handlerWrappers = new();

    public Mediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task<CreationResult> Send(ICommand<CreationResult> command, CancellationToken cancellationToken = default) =>
        await Send(command, typeof(CreationCommandHandlerWrapper<>), cancellationToken).ConfigureAwait(false);
    
    public async Task<CommandResultV2> Send(ICommand<CommandResultV2> command, CancellationToken cancellationToken = default) => 
        await Send(command, typeof(CommandHandlerWrapper<>), cancellationToken).ConfigureAwait(false);

    private async Task<TResult> Send<TResult>(ICommand<TResult> command, Type openWrapperType, CancellationToken cancellationToken)
    {
        var wrapper = (ICommandHandlerWrapper<TResult>)_handlerWrappers.GetOrAdd(command.GetType(), commandType =>
        {
            var wrapperType = openWrapperType.MakeGenericType(commandType);
            return Activator.CreateInstance(wrapperType) ?? throw new DispatcherException($"Could not create wrapper type for {commandType}");
        });

        return await wrapper.Handle(command, _serviceProvider, cancellationToken);
    }
}