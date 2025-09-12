using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.DDD.Core;

internal class ChangeMultiHandlerInternal<TAggregate, TCommand, THandler>(
    IAggregateRepository<TAggregate> repository,
    THandler handler) : ICommandHandler<TCommand, Unit>, IHandlerTypeProvider
    where TAggregate : Aggregate<TAggregate>
    where THandler : IChangeMultiHandler<TAggregate, TCommand>
{
    /// <summary>
    /// Handle incoming command and produces events
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IEnumerable<object> Events, Unit Result) Handle(TCommand command)
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