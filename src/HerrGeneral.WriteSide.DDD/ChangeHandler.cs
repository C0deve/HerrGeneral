using HerrGeneral.WriteSide.DDD.Exception;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Handler for a ChangeAggregate command
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
public abstract class ChangeHandler<TAggregate, TCommand>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Change<TAggregate>
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
    /// <param name="ctorParams"></param>
    protected ChangeHandler(CtorParams ctorParams) =>
        _repository = ctorParams.Repository;


    /// <summary>
    /// Handle incoming command and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public IEnumerable<object> Handle(TCommand command)
    {
        var aggregate = GetAggregate(command);
        aggregate = Handle(aggregate, command);
        _repository.Save(aggregate, command.Id);
        aggregate.ClearNewEvents();
        return aggregate.NewEvents;
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