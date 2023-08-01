using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public abstract class CommandHandlerBase<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    protected readonly IEventDispatcher EventDispatcher;

    internal CommandHandlerBase(IEventDispatcher eventDispatcher) =>
        EventDispatcher = eventDispatcher;

    public abstract Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    

    protected async Task Publish(IEvent @event, CancellationToken cancellationToken) => await EventDispatcher.Dispatch(@event, cancellationToken);
}