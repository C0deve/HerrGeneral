namespace HerrGeneral.DDD;

using WriteSide;

/// <summary>
/// Specifies that a property represents a lock key constrained to an aggregate type.
/// </summary>
/// <typeparam name="TAggregate">Type of the target aggregate.</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AggregateLockKeyAttribute<TAggregate>() : LockKeyAttribute<TAggregate>
    where TAggregate : IAggregate;
