using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

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
    /// <returns>A task containing the result with the GUID of the created aggregate</returns>
    /// <example>
    /// <code>
    /// Guid personId = await new CreatePerson("John", "Doe").SendFromMediator(mediator);
    /// </code>
    /// </example>
    public static Task<Result<Guid>> SendFromMediator<TAggregate>(this Create<TAggregate> command, Mediator mediator)
        where TAggregate : IAggregate => mediator.Send<Guid>(command);
    
    /// <summary>
    /// Sends a change command through the mediator using a fluent syntax.
    /// This method modifies an existing aggregate entity.
    /// </summary>
    /// <param name="command">The change command to send</param>
    /// <param name="mediator">The mediator instance used to process the command</param>
    /// <typeparam name="TAggregate">The type of aggregate to modify</typeparam>
    /// <returns>A task containing the result of the command execution</returns>
    /// <example>
    /// <code>
    /// await new UpdatePerson(personId, "NewName").SendFromMediator(mediator);
    /// </code>
    /// </example>
    public static Task<Result> SendFromMediator<TAggregate>(this Change<TAggregate> command, Mediator mediator)
        where TAggregate : IAggregate => mediator.Send(command);
}