using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide.DDD.Utils;

internal static class Extensions
{
    internal static async Task SaveAndDispatch<TAggregate>(this TAggregate ag, Guid sourceCommandId, Func<IEvent, CancellationToken, Task> eventDispatcher, IAggregateRepository<TAggregate> repository)
        where TAggregate : Aggregate<TAggregate>
    {
        repository.Save(ag, sourceCommandId);

        foreach (var @event in ag.NewEvents)
            await eventDispatcher(@event, CancellationToken.None);

        ag.ClearNewEvents();
    }

    
}