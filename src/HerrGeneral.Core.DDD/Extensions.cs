namespace HerrGeneral.DDD;

/// <summary>
/// Extensions methods to send command
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Sends a create command through the mediator using a fluent syntax.
    /// This method creates a new aggregate entity and returns its identifier.
    /// </summary>
    /// <param name="command">The create command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TAggregate">The type of aggregate to create</typeparam>
    /// <typeparam name="TKey">The type of aggregate key</typeparam>
    /// <returns>A task containing the result with the identifier of the created aggregate</returns>
    public static Task<Result<TKey>> SendFrom<TAggregate, TKey>(this Create<TAggregate, TKey> command, Mediator mediator)
        where TAggregate : IAggregate<TKey>
        where TKey : notnull => mediator.Send<TKey>(command);

    /// <summary>
    /// Sends a create command with Guid key through the mediator using a fluent syntax.
    /// </summary>
    /// <param name="command">The create command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TAggregate">The type of aggregate to create</typeparam>
    /// <returns>A task containing the result with the GUID of the created aggregate</returns>
    public static Task<Result<Guid>> SendFrom<TAggregate>(this Create<TAggregate> command, Mediator mediator)
        where TAggregate : IAggregate<Guid> => mediator.Send<Guid>(command);
    
    /// <summary>
    /// Sends a change command through the mediator using a fluent syntax.
    /// This method modifies an existing aggregate entity.
    /// </summary>
    /// <param name="command">The change command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TAggregate">The type of aggregate to modify</typeparam>
    /// <typeparam name="TKey">The type of aggregate key</typeparam>
    /// <returns>A task containing the result of the command execution</returns>
    public static Task<Result> SendFrom<TAggregate, TKey>(this Change<TAggregate, TKey> command, Mediator mediator)
        where TAggregate : IAggregate<TKey>
        where TKey : notnull => mediator.Send(command);

    /// <summary>
    /// Sends a change command with Guid key through the mediator using a fluent syntax.
    /// </summary>
    /// <param name="command">The change command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TAggregate">The type of aggregate to modify</typeparam>
    /// <returns>A task containing the result of the command execution</returns>
    public static Task<Result> SendFrom<TAggregate>(this Change<TAggregate> command, Mediator mediator)
        where TAggregate : IAggregate<Guid> => mediator.Send(command);
}