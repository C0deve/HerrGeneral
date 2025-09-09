using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.Bank.WriteSide;

public interface IAggregateRepository<T> : HerrGeneral.WriteSide.DDD.IAggregateRepository<T> 
    where T : IAggregate
{
    /// <summary>
    /// Find aggregates based on a given specification.
    /// </summary>
    /// <param name="func">A predicate function representing the specification to filter aggregates.</param>
    /// <returns>A collection of aggregates matching the specified criteria.</returns>
    IEnumerable<T> FindBySpecification(Func<T, bool> func);
}