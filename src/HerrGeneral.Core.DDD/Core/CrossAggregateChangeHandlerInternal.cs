using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

internal class CrossAggregateChangeHandlerInternal<TEvent, THandler, TAggregate>(
    IAggregateRepository<TAggregate> repository,
    THandler handler) : IEventHandler<TEvent>, IHandlerTypeProvider
    where TAggregate : Aggregate<TAggregate>
    where THandler : ICrossAggregateChangeHandler<TEvent, TAggregate>
{
    /// <summary>
    /// Handle incoming event and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public IEnumerable<object> Handle(TEvent command) =>
        handler
            .Handle(command)
            .Actions
            .Aggregate(new List<object>(), (acc, modificationRequest) =>
            {
                foreach (var aggregate in modificationRequest
                             .AggregateIds
                             .Select(repository.Get))
                {
                    modificationRequest.UpdateAction(aggregate);
                    repository.Save(aggregate);
                    acc.AddRange(aggregate.NewEvents);
                    aggregate.ClearNewEvents();
                }

                return acc;
            });

    public Type GetHandlerType() => typeof(THandler);
}