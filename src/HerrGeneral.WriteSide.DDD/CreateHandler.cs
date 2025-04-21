namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Handler for a CreateAggregate command
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public abstract class CreateHandler<TAggregate, TCommand> : ICommandHandler<TCommand, Guid>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Create<TAggregate>
{
    private readonly IAggregateRepository<TAggregate> _repository;

    // ReSharper disable once ClassNeverInstantiated.Global

    /// <summary>
    /// Parameters injected in the constructor
    /// </summary>
    /// <param name="Repository"></param>
    public record CtorParams(IAggregateRepository<TAggregate> Repository);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="params"></param>
    protected CreateHandler(CtorParams @params) =>
        _repository = @params.Repository;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public (IEnumerable<object> Events, Guid Result) Handle(TCommand command, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var aggregate = Handle(command, id);
        _repository.Save(aggregate, command.Id);
        var result = (aggregate.NewEvents, aggregate.Id);
        aggregate.ClearNewEvents();
        return result;
    }


    /// <summary>
    /// Create the aggregate
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    protected abstract TAggregate Handle(TCommand command, Guid aggregateId);
}