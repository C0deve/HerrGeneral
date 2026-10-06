using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

internal sealed class ChangeMultiHandlerInternal<TAggregate, TKey, TCommand, THandler>(
    IAggregateRepository<TAggregate, TKey> repository,
    THandler handler) : ICommandHandler<TCommand, Unit>, IHandlerTypeProvider
    where TAggregate : Aggregate<TAggregate, TKey>
    where THandler : IChangeMultiHandler<TAggregate, TCommand, TKey>
    where TKey : notnull
{
    /// <summary>
    /// Handle incoming command and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IReadOnlyList<object> Events, Unit Result) Handle(TCommand command)
    {
        var newEvents = handler
            .Handle(command)
            .Aggregate(new List<object>(), (acc, aggregate) =>
            {
                repository.Save(aggregate);
                acc.AddRange(aggregate.NewEvents);
                aggregate.ClearNewEvents();
                return acc;
            });

        return (newEvents, Unit.Default);
    }
    
    public Type GetHandlerType() => typeof(THandler);
}