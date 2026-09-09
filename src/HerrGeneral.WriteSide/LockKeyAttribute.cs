namespace HerrGeneral.WriteSide;

/// <summary>
/// Specifies that a property represents a lock/partition key for command concurrency control.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LockKeyAttribute : Attribute
{
    /// <summary>
    /// Gets the group used to qualify the lock key.
    /// </summary>
    public string KeyGroup { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="LockKeyAttribute"/> with a specific aggregate type qualification.
    /// </summary>
    /// <param name="keyGroup"></param>
    public LockKeyAttribute(string keyGroup)
    {
        KeyGroup = keyGroup ?? throw new ArgumentNullException(nameof(keyGroup));
    }
}

/// <summary>
/// Specifies that a property represents a lock/partition key for command concurrency control with generic type qualification.
/// </summary>
/// <typeparam name="TAggregate">The aggregate or domain type to qualify the key with.</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public class LockKeyAttribute<TAggregate>() : LockKeyAttribute(typeof(TAggregate).FullName ?? throw new InvalidOperationException());
