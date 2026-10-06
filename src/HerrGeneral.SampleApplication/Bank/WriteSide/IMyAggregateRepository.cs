using HerrGeneral.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface IMyAggregateRepository<TAggregate, TKey> : IAggregateRepository<TAggregate, TKey> 
    where TAggregate : IAggregate<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Find aggregates based on a given specification.
    /// </summary>
    /// <param name="func">A predicate function representing the specification to filter aggregates.</param>
    /// <returns>A collection of aggregates matching the specified criteria.</returns>
    IEnumerable<TAggregate> FindBySpecification(Func<TAggregate, bool> func);
}