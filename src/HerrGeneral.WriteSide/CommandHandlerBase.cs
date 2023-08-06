using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.WriteSide;

/// <summary>
/// Handler implementation for command
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public abstract class CommandHandlerBase<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    private readonly IEventDispatcher _eventDispatcher;

    internal CommandHandlerBase(IEventDispatcher eventDispatcher) =>
        _eventDispatcher = eventDispatcher;

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    

    /// <summary>
    /// Publish event on the write side
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    protected async Task Publish(IEvent @event, CancellationToken cancellationToken) => await _eventDispatcher.Dispatch(@event, cancellationToken);
}