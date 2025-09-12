using HerrGeneral.DDD;
using HerrGeneral.DDD.Exception;

namespace HerrGeneral.SampleApplication.Bank.Infrastructure;

public class Repository<TAggregate> : WriteSide.IMyAggregateRepository<TAggregate> 
    where TAggregate : IAggregate
{
    private readonly Dictionary<Guid, TAggregate> _aggregates = new();

    public TAggregate Get(Guid id)
    {
        _aggregates.TryGetValue(id, out var value);
        return value ?? throw new AggregateNotFound<TAggregate>(id);
    }

    public void Save(TAggregate aggregate) =>
        _aggregates[aggregate.Id] = aggregate;

    public IEnumerable<TAggregate> FindBySpecification(Func<TAggregate, bool> func) => 
        _aggregates.Values.Where(func);
}