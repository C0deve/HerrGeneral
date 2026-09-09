namespace HerrGeneral.WriteSide;

/// <summary>
/// Defines a command associated with a specific partition or aggregate key for concurrency control.
/// </summary>
public interface IKeyedCommand
{
    /// <summary>
    /// Gets the partition key for this command.
    /// Commands sharing the same key are executed sequentially to avoid concurrency conflicts,
    /// while commands with different keys can execute concurrently.
    /// </summary>
    object Key { get; }
}
