using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

/// <summary>
/// Internal Handler for a ChangeAggregate command
/// 1. Get the aggregate
/// 2. Handle the command
/// 3. Save aggregate
/// 4. Dispatch events
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <remarks>
/// Constructor
/// </remarks>
internal sealed class ChangeHandlerInternal<TAggregate, TKey, TCommand, THandler>(IAggregateRepository<TAggregate, TKey> repository, THandler handler) : ICommandHandler<TCommand, Unit>, IHandlerTypeProvider
    where TAggregate : Aggregate<TAggregate, TKey>
    where TCommand : Change<TAggregate, TKey>
    where THandler : IChangeHandler<TAggregate, TCommand, TKey>
    where TKey : notnull
{
    private readonly IAggregateRepository<TAggregate, TKey> _repository = repository;
    private readonly IChangeHandler<TAggregate, TCommand, TKey> _handler = handler;

    /// <summary>
    /// Handle incoming command and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IReadOnlyList<object> Events, Unit Result) Handle(TCommand command)
    {
        var aggregate = GetAggregate(command);
        aggregate = _handler.Handle(aggregate, command);
        _repository.Save(aggregate);
        var result = ((IReadOnlyList<object>)aggregate.NewEvents, Unit.Default);
        aggregate.ClearNewEvents();

        return result;
    }

    private TAggregate GetAggregate(TCommand command) =>
        _repository.Get(command.AggregateId);

    public Type GetHandlerType() => typeof(THandler);
}