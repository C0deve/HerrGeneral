using HerrGeneral.WriteSide.DDD.Utils;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Handler for a CreateAggregate command
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreateAggregateHandler<TAggregate, TCommand> : CreateHandler<TCommand>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : CreateAggregate<TAggregate>
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
    /// <param name="params"></param>
    protected CreateAggregateHandler(CtorParams @params) : base(@params.EventDispatcher) =>
        _repository = @params.Repository;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public sealed override async Task<CreateResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var aggregate = Handle(command, id);
        await aggregate.SaveAndDispatch(command.Id, Publish, _repository);
        return CreateResult.Success(aggregate.Id);
    }


    /// <summary>
    /// Create the aggregate
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    protected abstract TAggregate Handle(TCommand command, Guid aggregateId);
}