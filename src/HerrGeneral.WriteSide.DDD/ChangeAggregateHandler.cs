using HerrGeneral.WriteSide.DDD.Exception;
using HerrGeneral.WriteSide.DDD.Utils;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Handler for a ChangeAggregate command
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public abstract class ChangeAggregateHandler<TAggregate, TCommand> : ChangeHandler<TCommand>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : ChangeAggregate<TAggregate>
{
    private readonly IAggregateRepository<TAggregate> _repository;

    // ReSharper disable once ClassNeverInstantiated.Global
    
    /// <summary>
    /// Parameters injected in the constructor
    /// </summary>
    /// <param name="EventDispatcher"></param>
    /// <param name="Repository"></param>
    public record CtorParams(IEventDispatcher EventDispatcher, IAggregateRepository<TAggregate> Repository);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ctorParams"></param>
    protected ChangeAggregateHandler(CtorParams ctorParams) : base(ctorParams.EventDispatcher) =>
        _repository = ctorParams.Repository;


    /// <summary>
    ///
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public sealed override async Task<ChangeResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var aggregate = GetAggregate(command);

        await Handle(aggregate, command)
            .SaveAndDispatch(command.Id, Publish, _repository);
    
        return ChangeResult.Success;
    }

    private TAggregate GetAggregate(TCommand command) =>
        _repository.Get(command.AggregateId, command.Id) ?? throw new AggregateNotFound<TAggregate>(command.AggregateId);

    /// <summary>
    /// Edit the aggregate
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    protected abstract TAggregate Handle(TAggregate aggregate, TCommand command);
}