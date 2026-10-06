using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

/// <summary>
/// Internal handler for an aggregate creation
/// 1. Handle the command
/// 2. Save aggregate
/// 3. Dispatch events
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="repository"></param>
/// <param name="handler"></param>
internal sealed class CreateHandlerInternal<TAggregate, TKey, TCommand, THandler>(IAggregateRepository<TAggregate, TKey> repository, THandler handler) : ICommandHandler<TCommand, TKey>, IHandlerTypeProvider
    where TAggregate : Aggregate<TAggregate, TKey>
    where TCommand : Create<TAggregate, TKey>
    where THandler : ICreateHandler<TAggregate, TCommand, TKey>
    where TKey : notnull
{
    private readonly IAggregateRepository<TAggregate, TKey> _repository = repository;
    private readonly THandler _handler = handler;

    /// <summary>
    /// Handle the command and return events and the aggregate id.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public (IReadOnlyList<object> Events, TKey Result) Handle(TCommand command)
    {
        var id = GenerateOrExtractId(command);
        var aggregate = _handler.Handle(command, id);
        _repository.Save(aggregate);
        var result = ((IReadOnlyList<object>)aggregate.NewEvents, aggregate.Id);
        aggregate.ClearNewEvents();
        return result;
    }

    private static TKey GenerateOrExtractId(TCommand command)
    {
        if (typeof(TKey) == typeof(Guid))
        {
            var prop = typeof(TCommand).GetProperty("AggregateId") ?? typeof(TCommand).GetProperty("Id");
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                var val = (Guid)prop.GetValue(command)!;
                if (val != Guid.Empty)
                    return (TKey)(object)val;
            }
            return (TKey)(object)Guid.NewGuid();
        }

        var idProp = typeof(TCommand).GetProperty("AggregateId") ?? typeof(TCommand).GetProperty("Id");
        if (idProp != null && typeof(TKey).IsAssignableFrom(idProp.PropertyType))
        {
            var val = idProp.GetValue(command);
            if (val is TKey typedVal)
                return typedVal;
        }

        if (typeof(TKey) == typeof(string))
        {
            return (TKey)(object)Guid.NewGuid().ToString();
        }

        return default!;
    }
    
    public Type GetHandlerType() => typeof(THandler);
}