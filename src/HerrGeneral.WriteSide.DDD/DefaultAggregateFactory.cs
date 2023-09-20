using System.Reflection;

namespace HerrGeneral.WriteSide.DDD;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DefaultAggregateFactory<TAggregate> : IAggregateFactory<TAggregate> where TAggregate : IAggregate
{
    public TAggregate Create(Create<TAggregate> command, Guid aggregateId)
    {
        try
        {
            return (TAggregate)(Activator.CreateInstance(typeof(TAggregate),
                                    BindingFlags.NonPublic |BindingFlags.Public | BindingFlags.Instance, null,
                                    new object[] { command, aggregateId }, null) ??
                                throw new InvalidOperationException());
        }
        catch (MissingMethodException e)
        {
            throw new InvalidOperationException(
                $"{nameof(DefaultAggregateFactory<TAggregate>)} | Cannot find internal constructor new {typeof(TAggregate)}({command.GetType()} command, {aggregateId.GetType()} aggregateId)",
                e);
        }
    }
}