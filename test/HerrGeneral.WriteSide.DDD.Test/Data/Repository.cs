namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class Repository<TAggregate> : IAggregateRepository<TAggregate> where TAggregate : IAggregate
{
    private readonly Dictionary<Guid, TAggregate> _aggregates = new(); 
    public TAggregate? Get(Guid id)
    {
        _aggregates.TryGetValue(id, out var value);
        return value;
    }

    public void Save(TAggregate aggregate) => 
        _aggregates[aggregate.Id] = aggregate;
}