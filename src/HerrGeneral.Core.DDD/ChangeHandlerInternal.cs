using HerrGeneral.Core.DDD.Exception;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Internal Handler for a ChangeAggregate command
/// 1. Get the aggregate
/// 2. Handle the command
/// 3. Save aggregate
/// 4. Dispatch events
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
internal class ChangeHandlerInternal<TAggregate, TCommand, THandler> : ICommandHandler<TCommand, Unit>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Change<TAggregate>
    where THandler : IChangeHandler<TAggregate, TCommand>
{
    private readonly IAggregateRepository<TAggregate> _repository;
    private readonly IChangeHandler<TAggregate, TCommand> _handler;

    /// <summary>
    /// Constructor
    /// </summary>
    public ChangeHandlerInternal(IAggregateRepository<TAggregate> repository, THandler handler)
    {
        _repository = repository;
        _handler = handler;
    }

    /// <summary>
    /// Handle incoming command and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IEnumerable<object> Events, Unit Result) Handle(TCommand command)
    {
        var aggregate = GetAggregate(command);
        aggregate = _handler.Handle(aggregate, command);
        _repository.Save(aggregate);
        var result = (aggregate.NewEvents, Unit.Default);
        aggregate.ClearNewEvents();

        return result;
    }

    private TAggregate GetAggregate(TCommand command) =>
        _repository.Get(command.AggregateId) ?? throw new AggregateNotFound<TAggregate>(command.AggregateId);
}