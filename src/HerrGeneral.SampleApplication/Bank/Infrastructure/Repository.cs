using HerrGeneral.DDD;
using HerrGeneral.DDD.Exception;

namespace HerrGeneral.SampleApplication.Bank.Infrastructure;

public class Repository<TAggregate, TKey> : WriteSide.IMyAggregateRepository<TAggregate, TKey> 
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, TAggregate> _aggregates = new();

    public TAggregate Get(TKey id)
    {
        _aggregates.TryGetValue(id, out var value);
        return value ?? throw new AggregateNotFound<TAggregate, TKey>(id);
    }

    public void Save(TAggregate aggregate) =>
        _aggregates[aggregate.Id] = aggregate;

    public IEnumerable<TAggregate> FindBySpecification(Func<TAggregate, bool> func) => 
        _aggregates.Values.Where(func);
}