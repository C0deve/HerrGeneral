using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Internal handler for an aggregate creation
/// 1. Handle the command
/// 2. Save aggregate
/// 3. Dispatch events
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
internal class CreateHandlerInternal<TAggregate, TCommand, THandler> : ICommandHandler<TCommand, Guid>
    where TAggregate : Aggregate<TAggregate>
    where TCommand : Create<TAggregate>
    where THandler : ICreateHandler<TAggregate, TCommand>

{
    private readonly IAggregateRepository<TAggregate> _repository;
    private readonly THandler _handler;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="handler"></param>
    public CreateHandlerInternal(IAggregateRepository<TAggregate> repository, THandler handler)
    {
        _repository = repository;
        _handler = handler;
    }

    /// <summary>
    /// Handle the command and return events and the aggregate id.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IEnumerable<object> Events, Guid Result) Handle(TCommand command)
    {
        var id = Guid.NewGuid();
        var aggregate = _handler.Handle(command, id);
        _repository.Save(aggregate, command.Id);
        var result = (aggregate.NewEvents, aggregate.Id);
        aggregate.ClearNewEvents();
        return result;
    }
}