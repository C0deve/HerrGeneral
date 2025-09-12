using System.Reflection;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.DDD;

// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// Default aggregate factory
/// Call new TAggregate(Create command, Guid aggregateId)
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public class DefaultAggregateFactory<TAggregate> : IAggregateFactory<TAggregate> where TAggregate : IAggregate
{
    /// <summary>
    /// Call new TAggregate(Create command, Guid aggregateId)
    /// The constructor can be private or internal
    /// </summary>
    /// <param name="command"></param>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throw InvalidOperationException if the constructor new TAggregate(Create command, Guid aggregateId) is not found</exception>
    public TAggregate Create(Create<TAggregate> command, Guid aggregateId)
    {
        try
        {
            return (TAggregate)(Activator.CreateInstance(typeof(TAggregate),
                                    BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance, null,
                                    [command, aggregateId], null) ??
                                throw new InvalidOperationException());
        }
        catch (MissingMethodException e)
        {
            throw new MissingMethodException(
                $"Constructor new {typeof(TAggregate)}({command.GetType()} command, {aggregateId.GetType()} aggregateId) not found.",
                e);
        }
    }
}