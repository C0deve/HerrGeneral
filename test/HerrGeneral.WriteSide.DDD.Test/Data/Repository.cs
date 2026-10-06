using HerrGeneral.DDD;
using System.Collections.Concurrent;
using HerrGeneral.DDD.Exception;

namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class Repository<TAggregate, TKey> : IAggregateRepository<TAggregate, TKey>
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TAggregate> _aggregates = new();

    public TAggregate Get(TKey id)
    {
        _aggregates.TryGetValue(id, out var value);
        return value ?? throw new AggregateNotFound<TAggregate>(id);
    }

    public void Save(TAggregate aggregate) =>
        _aggregates[aggregate.Id] = aggregate;

    public IEnumerable<TAggregate> FindBySpecification(Func<TAggregate, bool> func) => 
        _aggregates.Values.Where(func);
}

public class Repository<TAggregate> : Repository<TAggregate, Guid>, IAggregateRepository<TAggregate>
    where TAggregate : IAggregate<Guid>
{
}